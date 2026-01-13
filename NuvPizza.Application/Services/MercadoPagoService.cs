using MercadoPago.Client.Preference;
using MercadoPago.Config;
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

            // ---------------------------------------------------------
            // CORREÇÃO: Use a URL HTTPS do Ngrok (copie do seu terminal)
            // O Mercado Pago BLOQUEIA http://localhost no AutoReturn
            // ---------------------------------------------------------
            string baseUrl = "https://coralliferous-gloatingly-song.ngrok-free.dev"; 
        
            var backUrls = new PreferenceBackUrlsRequest
            {
                Success = $"{baseUrl}/sucesso",
                Failure = $"{baseUrl}/checkout",
                Pending = $"{baseUrl}/checkout"
            };

            // --- DEBUG: Vamos ver se as URLs foram criadas ---
            Console.WriteLine($"[DEBUG] BackUrl Success definida como: {backUrls.Success}");
            // ------------------------------------------------

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
                    Email = !string.IsNullOrEmpty(dto.EmailPagador) ? dto.EmailPagador : "cliente@nuvpizza.com"
                },
                
                // 2. AQUI É O PONTO CRÍTICO. 
                // Se essa linha não existir ou estiver comentada, dá o erro 400.
                BackUrls = backUrls, 
                
                AutoReturn = "approved",
                ExternalReference = Guid.NewGuid().ToString(),
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

            // --- DEBUG FINAL ANTES DE ENVIAR ---
            if (request.BackUrls == null)
            {
                 Console.WriteLine("[ERRO CRITICO] O objeto request.BackUrls ESTÁ NULO! O erro vai acontecer agora.");
            }
            else
            {
                 Console.WriteLine("[SUCESSO] request.BackUrls foi preenchido corretamente.");
            }
            // -----------------------------------

            Preference preference = await client.CreateAsync(request);

            return Result<string>.Success(preference.SandboxInitPoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO MP DETALHADO: {ex.Message}");
            return Result<string>.Failure($"Erro Mercado Pago: {ex.Message}");
        }
    }
}