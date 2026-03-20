using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class CupomRepository : Repository<Cupom>, ICupomRepository
{
    public CupomRepository(AppDbContext context) : base(context) { }

}