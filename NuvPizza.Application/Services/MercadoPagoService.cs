using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Configuration;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;

namespace NuvPizza.Application.Services;

public class MercadoPagoService : IPagamentoService
{
    private readonly IConfiguration _configuration;

    public MercadoPagoService(IConfiguration configuration)
    {
        _configuration = configuration;
        MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];
    }

    public async Task<Result<PagamentoResponseDTO>> ProcessarPagamentoAsync(PagamentoRequestDTO dto)
    {
       try
        {
            // 1. Tratamento de email (mantido do seu código)
            string emailDoPagador = dto.Payer.Email;
            if (string.IsNullOrEmpty(emailDoPagador))
            {
                var telefoneLimpo = new string(dto.Payer.Phone.Where(char.IsDigit).ToArray());
                emailDoPagador = $"{telefoneLimpo}@cliente.nuvpizza.com.br";
            }

            // 2. Criação do Request (Serve para Cartão, GPay e Apple Pay)
            var request = new PaymentCreateRequest
            {
                TransactionAmount = dto.TransactionAmount,
                Token = dto.Token, // O Token vem do Front (seja digitado ou via GPay)
                Description = dto.Description,
                Installments = dto.Installments,
                PaymentMethodId = dto.PaymentMethodId,
                Payer = new PaymentPayerRequest
                {
                    Email = emailDoPagador, 
                    FirstName = dto.Payer.FirstName,
                    Identification = new IdentificationRequest
                    {
                        Type = dto.Payer.Identification.Type,
                        Number = dto.Payer.Identification.Number
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    { "pedido_id", dto.PedidoId }
                },
                // Essa opção força o MP a dar uma resposta final (Aprovado ou Recusado)
                // Evita o status "Pending" que é ruim para pizzaria
                BinaryMode = true 
            };

            if (!string.IsNullOrEmpty(dto.IssuerId))
            {
                request.IssuerId = dto.IssuerId;
            }

            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(request);

            var responseDto = new PagamentoResponseDTO
            {
                PaymentId = payment.Id.GetValueOrDefault(),
                Status = payment.Status,
                StatusDetail = payment.StatusDetail
            };

            // Lógica para Pix
            if (dto.PaymentMethodId.ToLower() == "pix" && payment.PointOfInteraction != null)
            {
                responseDto.QrCodeBase64 = payment.PointOfInteraction.TransactionData.QrCodeBase64;
                responseDto.QrCodeCopiaCola = payment.PointOfInteraction.TransactionData.QrCode;
            }

            // 3. Verificação de Sucesso
            // Com BinaryMode=true, dificilmente cairá em Pending/InProcess, mas mantemos por segurança
            if (payment.Status == PaymentStatus.Approved || 
                payment.Status == PaymentStatus.InProcess) 
            {
                return Result<PagamentoResponseDTO>.Success(responseDto);
            }

            // Se recusou, traduz o erro
            string mensagemAmigavel = TraduzirDetalheStatus(payment.StatusDetail);
            return Result<PagamentoResponseDTO>.Failure(mensagemAmigavel);
        }
        catch (MercadoPago.Error.MercadoPagoApiException mpEx)
        {
            // Captura erros da API (ex: cartão recusado, token inválido)
            var msg = mpEx.ApiError?.Message ?? mpEx.Message;
            Console.WriteLine($"Erro MP: {msg}");
            
            // Tenta traduzir o erro se possível, senão manda genérico
            if(mpEx.ApiError?.Cause != null && mpEx.ApiError.Cause.Count > 0)
                return Result<PagamentoResponseDTO>.Failure($"Erro: {mpEx.ApiError.Cause[0].Description}");

            return Result<PagamentoResponseDTO>.Failure("Não foi possível processar o pagamento. Verifique os dados.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro Interno: {ex.Message}");
            return Result<PagamentoResponseDTO>.Failure($"Erro interno no servidor.");
        }
    }

    private string TraduzirDetalheStatus(string statusDetail)
    {
        return statusDetail switch
        {
            "cc_rejected_bad_filled_card_number" or 
            "cc_rejected_bad_filled_date" or 
            "cc_rejected_bad_filled_other" or 
            "cc_rejected_bad_filled_security_code" => "Revise os dados do cartão.",
            "cc_rejected_insufficient_amount" => "Saldo insuficiente.",
            "cc_rejected_call_for_authorize" => "Autorize o pagamento junto ao seu banco.",
            "cc_rejected_card_disabled" => "Cartão bloqueado ou desabilitado.",
            "cc_rejected_invalid_installments" => "Número de parcelas não aceito.",
            "cc_rejected_high_risk" => "Pagamento recusado por segurança.",
            _ => "Pagamento recusado. Tente outro cartão."
        };
    }
}