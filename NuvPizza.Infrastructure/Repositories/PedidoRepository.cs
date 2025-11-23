using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(AppDbContext context) : base(context) { }
    
}