using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<PagedResult<Pedido>> GetAllWithDetailsAsync(PedidoParameters pedidoParametersParameters);
}