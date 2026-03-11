using Microsoft.Extensions.Diagnostics.HealthChecks;
using NuvPizza.Infrastructure.Services;

namespace NuvPizza.API.HealthChecks;

public class ViaCepHealthCheck : IHealthCheck
{
    private readonly IViaCepService _viaCepService;

    public ViaCepHealthCheck(IViaCepService viaCepService)
    {
        _viaCepService = viaCepService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var cepTeste = "59074-576"; 
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _viaCepService.CheckAsync(cepTeste);
            watch.Stop();
            
            if (response == null)
            {
                return HealthCheckResult.Unhealthy($"A API do ViaCep falhou.");
            }
            
            if (watch.ElapsedMilliseconds > 1500)
            {
                return HealthCheckResult.Degraded($"O ViaCep respondeu, mas está lento. Tempo: {watch.ElapsedMilliseconds}ms");
            }

            return HealthCheckResult.Healthy($"ViaCep operante. Latência: {watch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro fatal ao tentar conectar no ViaCep.", ex);
        }
    }
}