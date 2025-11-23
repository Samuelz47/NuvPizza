using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(AppDbContext context) : base(context) { }
}