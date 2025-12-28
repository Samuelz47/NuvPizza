using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class ConfiguracaoRepository : Repository<Configuracao>, IConfiguracaoRepository
{
    public ConfiguracaoRepository(AppDbContext context) : base(context) { }
}