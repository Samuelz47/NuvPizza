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
            // Se o front não mandou e-mail, geramos um fictício com o telefone para o MP aceitar.
            string emailDoPagador = dto.Payer.Email;

            if (string.IsNullOrEmpty(emailDoPagador))
            {
                // Remove tudo que não for número do telefone (para evitar erros com ( ) - )
                var telefoneLimpo = new string(dto.Payer.Phone.Where(char.IsDigit).ToArray());
                
                // Cria: 11999999999@cliente.nuvpizza.com.br
                emailDoPagador = $"{telefoneLimpo}@cliente.nuvpizza.com.br";
            }

            var request = new PaymentCreateRequest
            {
                TransactionAmount = dto.TransactionAmount,
                Token = dto.Token, // Se for Pix, o SDK ignora isso automaticamente
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

            // Se for PIX, extraímos os dados do QR Code
            if (dto.PaymentMethodId.ToLower() == "pix" && payment.PointOfInteraction != null)
            {
                responseDto.QrCodeBase64 = payment.PointOfInteraction.TransactionData.QrCodeBase64; // Imagem
                responseDto.QrCodeCopiaCola = payment.PointOfInteraction.TransactionData.QrCode;    // Texto
            }

            // Consideramos sucesso se foi Aprovado (Cartão) ou Pendente/Em Processo (Pix/Cartão em análise)
            if (payment.Status == PaymentStatus.Approved || 
                payment.Status == PaymentStatus.Pending || 
                payment.Status == PaymentStatus.InProcess)
            {
                return Result<PagamentoResponseDTO>.Success(responseDto);
            }

            return Result<PagamentoResponseDTO>.Failure($"Pagamento rejeitado: {payment.StatusDetail}");
        }
        catch (Exception ex)
        {
            return Result<PagamentoResponseDTO>.Failure($"Erro ao processar pagamento: {ex.Message}");
        }
    }
}