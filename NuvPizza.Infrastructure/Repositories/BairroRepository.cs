using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class BairroRepository : Repository<Bairro>, IBairroRepository
{
    public BairroRepository(AppDbContext context) : base(context) { }
    public async Task<IEnumerable<Bairro>> GetAllAsync()
    {
        return await _context.Bairros.Where(b => b.Ativo).AsNoTracking().ToListAsync();
    }
}