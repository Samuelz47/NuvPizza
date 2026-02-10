using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Domain.Enums;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Application.Interfaces;

public interface IPedidoService
{
    Task<Result<PedidoDTO>> CreatePedidoAsync(PedidoForRegistrationDTO PedidoForRegistrationDTO);
    Task<Result<PedidoDTO>> UpdateStatusPedidoAsync(Guid Id, StatusPedidoForUpdateDTO newStatus);
    Task<PagedResult<PedidoDTO>> GetAllPedidosAsync(PedidoParameters pedidoParameters);
    Task<Result<PedidoDTO>> GetPedidoByIdAsync(Guid pedidoId);
    Task<Result<PedidoDTO>> ConfirmarPagamentoAsync(Guid pedidoId, FormaPagamento formaPagamento);
}