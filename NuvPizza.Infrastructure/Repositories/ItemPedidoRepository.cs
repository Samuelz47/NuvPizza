using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class ItemPedidoRepository : Repository<ItemPedido>, IItemPedidoRepository
{
    public ItemPedidoRepository(AppDbContext context) : base(context) { }
}