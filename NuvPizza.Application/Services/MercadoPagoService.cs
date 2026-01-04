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
            string emailDoPagador = dto.Payer.Email;

            if (string.IsNullOrEmpty(emailDoPagador))
            {
                var telefoneLimpo = new string(dto.Payer.Phone.Where(char.IsDigit).ToArray());
                emailDoPagador = $"{telefoneLimpo}@cliente.nuvpizza.com.br";
            }

            var request = new PaymentCreateRequest
            {
                TransactionAmount = dto.TransactionAmount,
                Token = dto.Token,
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
                }
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

            if (dto.PaymentMethodId.ToLower() == "pix" && payment.PointOfInteraction != null)
            {
                responseDto.QrCodeBase64 = payment.PointOfInteraction.TransactionData.QrCodeBase64;
                responseDto.QrCodeCopiaCola = payment.PointOfInteraction.TransactionData.QrCode;
            }

            if (payment.Status == PaymentStatus.Approved || 
                payment.Status == PaymentStatus.Pending || 
                payment.Status == PaymentStatus.InProcess)
            {
                return Result<PagamentoResponseDTO>.Success(responseDto);
            }

            string mensagemAmigavel = TraduzirDetalheStatus(payment.StatusDetail);
            
            return Result<PagamentoResponseDTO>.Failure(mensagemAmigavel);
        }
        catch (MercadoPago.Error.MercadoPagoApiException mpEx)
        {
            return Result<PagamentoResponseDTO>.Failure($"Erro nos dados do cartão: {mpEx.Message}");
        }
        catch (Exception ex)
        {
            return Result<PagamentoResponseDTO>.Failure($"Erro interno ao processar: {ex.Message}");
        }
    }

    private string TraduzirDetalheStatus(string statusDetail)
    {
        return statusDetail switch
        {
            // Qualquer erro de preenchimento retorna a mesma mensagem genérica
            "cc_rejected_bad_filled_card_number" or 
            "cc_rejected_bad_filled_date" or 
            "cc_rejected_bad_filled_other" or 
            "cc_rejected_bad_filled_security_code" => "Revise os dados do cartão.",

            // Erros específicos que não comprometem segurança
            "cc_rejected_insufficient_amount" => "O cartão possui saldo insuficiente.",
            "cc_rejected_call_for_authorize" => "Você precisa autorizar o pagamento com seu banco.",
            "cc_rejected_card_disabled" => "Ligue para o seu banco para ativar seu cartão.",
            "cc_rejected_invalid_installments" => "O cartão não processa pagamentos neste número de parcelas.",
            "cc_rejected_duplicated_payment" => "Pagamento duplicado. Aguarde alguns minutos.",
            "cc_rejected_max_attempts" => "Limite de tentativas atingido. Use outro cartão.",
            "cc_rejected_high_risk" => "Pagamento recusado por segurança. Escolha outra forma de pagamento.",
            "cc_rejected_blacklist" => "Não pudemos processar seu pagamento.",
            
            // Padrão
            _ => "Pagamento recusado. Tente outro meio de pagamento."
        };
    }
}