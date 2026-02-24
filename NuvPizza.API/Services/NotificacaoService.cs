using Microsoft.AspNetCore.SignalR;
using NuvPizza.API.Hubs;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;

namespace NuvPizza.API.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly IHubContext<NotificacaoHub> _hubContext;

    public NotificacaoService(IHubContext<NotificacaoHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarAlteracaoStatusLoja(bool estaAberta, string mensagem)
    {
        await _hubContext.Clients.All.SendAsync("LojaStatusAlterado", estaAberta, mensagem);
    }

    public async Task NotificarFechamento(int minutosRestantes)
    {
        await _hubContext.Clients.All.SendAsync("AlertaFechamento", minutosRestantes);
    }

    public async Task NotificarNovoPedido(PedidoDTO pedido)
    {
        await _hubContext.Clients.All.SendAsync("NovoPedidoRecebido", pedido);
    }

    public async Task NotificarAtualizacaoStatus(Guid pedidoId, int novoStatus)
    {
        await _hubContext.Clients.All.SendAsync("StatusPedidoAtualizado", pedidoId, novoStatus);
    }
}