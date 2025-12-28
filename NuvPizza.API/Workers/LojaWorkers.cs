using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuvPizza.API.Hubs;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.API.Workers;

public class LojaWorkers : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<NotificacaoHub> _hubContext;
    private readonly ILogger<LojaWorkers> _logger;

    public LojaWorkers(ILogger<LojaWorkers> logger, IHubContext<NotificacaoHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarStatusLojar();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Robô da loja");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task VerificarStatusLojar()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var config = await context.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);

            if (config == null || !config.EstaAberta || config.DataHoraFechamentoAtual == null)
            {
                return;
            }

            var agora = DateTime.Now;
            var horarioFechamento = config.DataHoraFechamentoAtual.Value;
            var tempoRestante = horarioFechamento - agora;

            if (tempoRestante.TotalMinutes <= 0)
            {
                config.EstaAberta = false;
                config.DataHoraFechamentoAtual = null;

                context.Configuracoes.Update(config);
                await context.SaveChangesAsync();

                _logger.LogInformation("Robô fechou a loja automaticamente às {Hora}", agora);

                await _hubContext.Clients.All.SendAsync("LojaStatusAlterado", false, "Fechado Automaticamente");
            }
            else if (tempoRestante.TotalMinutes <= 10 && tempoRestante.TotalMinutes > 0)
            {
                await _hubContext.Clients.All.SendAsync("AlertaFechamento", (int)tempoRestante.TotalMinutes);
            }
        }
    }
}