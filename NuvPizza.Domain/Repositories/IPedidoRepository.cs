using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<PagedResult<Pedido>> GetAllWithDetailsAsync(PedidoParameters pedidoParametersParameters);
    Task<Pedido?> GetByIdWithDetailsAsync(Guid pedidoId);
    Task<(decimal Total, decimal Frete, int Quantidade)>GetFaturamentoAsync(DateTime dataInicio, DateTime dataFim);
}