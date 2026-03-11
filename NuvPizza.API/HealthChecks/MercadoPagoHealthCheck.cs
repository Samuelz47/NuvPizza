using MercadoPago.Client.PaymentMethod;
using MercadoPago.Config;
using MercadoPago.Error;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;

namespace NuvPizza.API.HealthChecks;

public class MercadoPagoHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public MercadoPagoHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Garante que o Token de configuração global (do SDK) também está setado com o que vem dos Secrets
            var tokenSecret = _configuration["MercadoPago:AccessToken"];
            if (!string.IsNullOrEmpty(tokenSecret))
            {
                MercadoPagoConfig.AccessToken = tokenSecret;
            }

            var client = new PaymentMethodClient();
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            var paymentMethods = await client.ListAsync(cancellationToken: cancellationToken);
            
            watch.Stop();

            if (paymentMethods == null || !paymentMethods.Any())
            {
                return HealthCheckResult.Degraded("MercadoPago retornou 0 métodos de pagamento disponíveis.");
            }

            if (watch.ElapsedMilliseconds > 3000) 
            {
                return HealthCheckResult.Degraded($"MercadoPago operante, mas com latência alta: {watch.ElapsedMilliseconds}ms");
            }

            return HealthCheckResult.Healthy($"MercadoPago operante. {paymentMethods.Count} métodos carregados. Latência: {watch.ElapsedMilliseconds}ms");
        }
        catch (MercadoPagoApiException apiEx) 
        {
            var statusCode = (int?)apiEx.ApiResponse?.StatusCode;
             
            if (statusCode == 401)
            {
                return HealthCheckResult.Unhealthy("FALHA CRÍTICA: Access Token do Mercado Pago é INVÁLIDO ou foi revogado.");
            }
             
            return HealthCheckResult.Unhealthy($"Erro na API do Mercado Pago: {apiEx.Message}");
        }
        catch (MercadoPagoException mpEx)
        {
            return HealthCheckResult.Unhealthy($"Erro interno do SDK do Mercado Pago: {mpEx.Message}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Serviço do Mercado Pago inacessível por falha de rede/timeout.", ex);
        }
    }
}
