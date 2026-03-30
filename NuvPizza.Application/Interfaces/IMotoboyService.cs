using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;

namespace NuvPizza.Application.Interfaces
{
    public interface IMotoboyService
    {
        Task<Result<IEnumerable<MotoboyDTO>>> ObterTodosAsync();
        Task<Result<IEnumerable<MotoboyDTO>>> ObterAtivosAsync();
        Task<Result<MotoboyDTO>> ObterPorIdAsync(Guid id);
        Task<Result<MotoboyDTO>> CriarAsync(MotoboyCreateDTO dto);
        Task<Result<MotoboyDTO>> AtualizarAsync(MotoboyUpdateDTO dto);
        Task<Result<bool>> DeletarAsync(Guid id);
        Task<Result<FaturamentoMotoboyDTO>> ObterFaturamentoIndividualAsync(Guid motoboyId, DateTime dataInicial, DateTime dataFinal);
    }
}
