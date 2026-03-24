using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(AppDbContext context) : base(context) { }

    public async Task<Cliente?> GetByTelefoneAsync(string telefone)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Telefone == telefone);
    }

    public async Task<PagedResult<Cliente>> GetRankingAsync(ClienteRankingParameters parameters)
    {
        var query = _context.Clientes.AsNoTracking().AsQueryable();

        query = parameters.OrdenarPor?.ToLower() == "pedidos"
            ? query.OrderByDescending(c => c.QuantidadePedidos)
            : query.OrderByDescending(c => (double)c.ValorTotalGasto); // cast to double because SQLite doesn't support sorting on decimal

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<Cliente>(items, parameters.PageNumber, parameters.PageSize, totalCount);
    }
}
