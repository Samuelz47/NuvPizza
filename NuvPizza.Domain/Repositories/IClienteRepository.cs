using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Domain.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByTelefoneAsync(string telefone);
    Task<PagedResult<Cliente>> GetRankingAsync(ClienteRankingParameters parameters);
}
