using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces;

public interface IFaturamentoService
{
    Task<Result<FaturamentoDTO>> ObterFaturamentoAsync(DateTime dataInicial, DateTime dataFinal);
}