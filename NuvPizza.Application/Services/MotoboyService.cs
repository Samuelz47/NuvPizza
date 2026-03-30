using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services
{
    public class MotoboyService : IMotoboyService
    {
        private readonly IMotoboyRepository _motoboyRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;

        public MotoboyService(IMotoboyRepository motoboyRepository, IPedidoRepository pedidoRepository, IMapper mapper)
        {
            _motoboyRepository = motoboyRepository;
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<MotoboyDTO>>> ObterTodosAsync()
        {
            var motoboys = await _motoboyRepository.GetAllAsync();
            return Result<IEnumerable<MotoboyDTO>>.Success(_mapper.Map<IEnumerable<MotoboyDTO>>(motoboys));
        }

        public async Task<Result<IEnumerable<MotoboyDTO>>> ObterAtivosAsync()
        {
            var motoboys = await _motoboyRepository.GetAtivosAsync();
            return Result<IEnumerable<MotoboyDTO>>.Success(_mapper.Map<IEnumerable<MotoboyDTO>>(motoboys));
        }

        public async Task<Result<MotoboyDTO>> ObterPorIdAsync(Guid id)
        {
            var motoboy = await _motoboyRepository.GetByIdAsync(id);
            if (motoboy == null) return Result<MotoboyDTO>.Failure("Motoboy não encontrado");
            return Result<MotoboyDTO>.Success(_mapper.Map<MotoboyDTO>(motoboy));
        }

        public async Task<Result<MotoboyDTO>> CriarAsync(MotoboyCreateDTO dto)
        {
            var motoboy = _mapper.Map<Motoboy>(dto);
            await _motoboyRepository.AddAsync(motoboy);
            return Result<MotoboyDTO>.Success(_mapper.Map<MotoboyDTO>(motoboy));
        }

        public async Task<Result<MotoboyDTO>> AtualizarAsync(MotoboyUpdateDTO dto)
        {
            var motoboy = await _motoboyRepository.GetByIdAsync(dto.Id);
            if (motoboy == null) return Result<MotoboyDTO>.Failure("Motoboy não encontrado");

            _mapper.Map(dto, motoboy);
            await _motoboyRepository.UpdateAsync(motoboy);
            return Result<MotoboyDTO>.Success(_mapper.Map<MotoboyDTO>(motoboy));
        }

        public async Task<Result<bool>> DeletarAsync(Guid id)
        {
            await _motoboyRepository.DeleteAsync(id);
            return Result<bool>.Success(true);
        }

        public async Task<Result<FaturamentoMotoboyDTO>> ObterFaturamentoIndividualAsync(Guid motoboyId, DateTime dataInicial, DateTime dataFinal)
        {
            var motoboy = await _motoboyRepository.GetByIdAsync(motoboyId);
            if (motoboy == null) return Result<FaturamentoMotoboyDTO>.Failure("Motoboy não encontrado");

            // Buscar pedidos do motoboy no período que estão entregues? 
            // Precisamos saber quais status indicam que o frete foi "feito". Geralmente 'Entregue'.
            // Mas o usuário disse "trabalha com o frete sendo 100% do motoboy".
            
            // Vou precisar de um método em PedidoRepository para buscar faturamento por motoboy.
            var pedidosNoPeriodo = await _pedidoRepository.GetPedidosNoPeriodoAsync(dataInicial, dataFinal);
            var pedidosMotoboy = pedidosNoPeriodo.Where(p => p.MotoboyId == motoboyId);

            var faturamento = new FaturamentoMotoboyDTO
            {
                MotoboyId = motoboyId,
                NomeMotoboy = motoboy.Nome,
                TotalFrete = pedidosMotoboy.Sum(p => p.ValorFrete),
                QuantidadeEntregas = pedidosMotoboy.Count(),
                DataInicial = dataInicial,
                DataFinal = dataFinal
            };

            return Result<FaturamentoMotoboyDTO>.Success(faturamento);
        }
    }
}
