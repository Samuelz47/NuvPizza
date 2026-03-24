using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Application.Interfaces;

public interface IClienteService
{
    Task<Guid> EnsureClienteExistsAsync(string telefone, string nome, string? email);
    Task AddPedidoToRankingAsync(Guid clienteId, decimal valorPedido);
    Task<PagedResult<ClienteDTO>> GetRankingAsync(ClienteRankingParameters parameters);
}
