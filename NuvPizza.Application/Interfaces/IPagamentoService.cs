using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface IPagamentoService
{
    Task<Result<PagamentoResponseDTO>> ProcessarPagamentoAsync(PagamentoRequestDTO pagamentoDto);
}