using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
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

    public async Task<Result<string>> CriarPreferenciaAsync(CriarPreferenceDTO dto)
    {
        try
        {
            var client = new PreferenceClient();

            // Mantenha a URL do Ngrok se estiver usando, ou localhost se for só teste local
            // Se estiver usando Ngrok, substitua a linha abaixo
            string baseUrl = "https://coralliferous-gloatingly-song.ngrok-free.dev"; 
            // string baseUrl = "http://localhost:4200"; 
        
            var backUrls = new PreferenceBackUrlsRequest
            {
                Success = $"{baseUrl}/sucesso",
                Failure = $"{baseUrl}/checkout",
                Pending = $"{baseUrl}/checkout"
            };

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = dto.Titulo,
                        Quantity = dto.Quantidade,
                        CurrencyId = "BRL",
                        UnitPrice = dto.PrecoUnitario,
                    }
                },
                Payer = new PreferencePayerRequest
                {
                    // Mantivemos o email aleatório para o Sandbox não bloquear por "vendedor comprando de si mesmo"
                    Email = $"teste_{Guid.NewGuid().ToString().Substring(0, 8)}@nuvpizza.com"
                },
                
                NotificationUrl = $"{baseUrl}/api/pagamento/webhook",
                BackUrls = backUrls,
                AutoReturn = "approved",
                
                // -------------------------------------------------------------
                // AQUI ESTÁ A CORREÇÃO QUE VOCÊ QUER:
                // Usa o ID do Pedido que veio do DTO.
                // -------------------------------------------------------------
                ExternalReference = !string.IsNullOrEmpty(dto.ExternalReference) 
                                    ? dto.ExternalReference 
                                    : Guid.NewGuid().ToString(), // Fallback só se vier vazio
                
                StatementDescriptor = "NUVPIZZA",
                Expires = false,
                PaymentMethods = new PreferencePaymentMethodsRequest
                {
                    ExcludedPaymentTypes = new List<PreferencePaymentTypeRequest>
                    {
                        new PreferencePaymentTypeRequest { Id = "ticket" }
                    },
                    Installments = 1
                }
            };

            Preference preference = await client.CreateAsync(request);

            // Retorna o link de SANDBOX
            return Result<string>.Success(preference.SandboxInitPoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO MP DETALHADO: {ex.Message}");
            return Result<string>.Failure($"Erro Mercado Pago: {ex.Message}");
        }
    }

    public async Task<Result<StatusPagamentoDTO>> ConsultarStatusPagamentoAsync(string pagamentoId)
    {
        try
        {
            if (!long.TryParse(pagamentoId, out long idLong)) return Result<StatusPagamentoDTO>.Failure("ID de pagamento inválido");

            var client = new PaymentClient();
            Payment payment = await client.GetAsync(idLong);
            
            if (payment is null) return Result<StatusPagamentoDTO>.Failure("Pagamento não encontrado");

            return Result<StatusPagamentoDTO>.Success(new StatusPagamentoDTO
            {
                Status = payment.Status,
                PedidoIdExterno = payment.ExternalReference,
            });
        }
        catch (Exception ex)
        {
            return Result<StatusPagamentoDTO>.Failure($"Erro ao consultar MP: {ex.Message}");
        }
    }
}