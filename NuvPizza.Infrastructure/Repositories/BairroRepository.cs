using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class BairroRepository : Repository<Bairro>, IBairroRepository
{
    public BairroRepository(AppDbContext context) : base(context) { }
}