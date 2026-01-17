using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface INotificacaoService
{
    Task NotificarAlteracaoStatusLoja(bool estaAberta, string mensagem);
    Task NotificarFechamento(int minutosRestantes);
    Task NotificarNovoPedido(PedidoDTO pedido);
    Task NotificarAtualizacaoStatus(Guid pedidoId, int novoStatus);
}