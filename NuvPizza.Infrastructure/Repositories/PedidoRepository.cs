using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Pedido>> GetAllWithDetailsAsync(PedidoParameters pedidoParameters)
    {
        var query = _context.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .AsQueryable();

        if (pedidoParameters.Status != null && pedidoParameters.Status.Any())
            query = query.Where(p => pedidoParameters.Status.Contains(p.StatusPedido));


        if (pedidoParameters.DataInicio.HasValue)
            query = query.Where(p => p.DataPedido >= pedidoParameters.DataInicio.Value);

        if (pedidoParameters.DataFim.HasValue)
            query = query.Where(p => p.DataPedido <= pedidoParameters.DataFim.Value);


        if (pedidoParameters.ValorMinimo.HasValue)
            query = query.Where(p => p.ValorTotal >= pedidoParameters.ValorMinimo.Value);

        
        if (pedidoParameters.ValorMaximo.HasValue)
            query = query.Where(p => p.ValorTotal <= pedidoParameters.ValorMaximo.Value);
        

        query = query.OrderByDescending(p => p.DataPedido);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pedidoParameters.PageNumber - 1) * pedidoParameters.PageSize)
            .Take(pedidoParameters.PageSize)
            .ToListAsync();
        
        return new PagedResult<Pedido>(items, pedidoParameters.PageNumber, pedidoParameters.PageSize, totalCount);
    }
}