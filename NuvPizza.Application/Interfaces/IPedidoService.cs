using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface IPedidoService
{
    Task<Result<PedidoDTO>> CreatePedidoAsync(PedidoForRegistrationDTO PedidoForRegistrationDTO);
    Task<Result<PedidoDTO>> UpdateStatusPedidoAsync(Guid Id, StatusPedidoForUpdateDTO newStatus);
}