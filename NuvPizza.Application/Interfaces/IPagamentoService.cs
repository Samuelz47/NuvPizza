using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface IPagamentoService
{
    Task<Result<string>> CriarPreferenciaAsync(CriarPreferenceDTO dto);
    Task<Result<StatusPagamentoDTO>> ConsultarStatusPagamentoAsync(string pagamentoId);
}