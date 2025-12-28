using NuvPizza.Domain.Entities;

namespace NuvPizza.Domain.Repositories;

public interface IBairroRepository : IRepository<Bairro>
{
    Task<IEnumerable<Bairro>> GetAllAsync();
}