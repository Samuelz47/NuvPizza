using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services;

public class FaturamentoService : IFaturamentoService
{
    private readonly IPedidoRepository _pedidoRepository;

    public FaturamentoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<Result<FaturamentoDTO>> ObterFaturamentoAsync(DateTime dataInicial, DateTime dataFinal)
    {
        try
        {
            if (dataInicial > dataFinal) return Result<FaturamentoDTO>.Failure("Data inicial maior que a final");
            
            var resultado = await _pedidoRepository.GetFaturamentoAsync(dataInicial, dataFinal);

            decimal ticketMedio = 0;
            if (resultado.Quantidade > 0)
            {
                ticketMedio = resultado.Total /  resultado.Quantidade;
            }

            var dto = new FaturamentoDTO
            {
                Faturamento = resultado.Total,
                Frete = resultado.Frete,
                QuantidadePedidos = resultado.Quantidade,
                TicketMedio = ticketMedio
            };

            // Buscar pedidos no período para agrupar por motoboy
            var pedidos = await _pedidoRepository.GetPedidosNoPeriodoAsync(dataInicial, dataFinal);
            dto.FretesPorMotoboy = pedidos
                .Where(p => p.MotoboyId.HasValue && p.StatusPedido == Domain.Enums.StatusPedido.Entrega)
                .GroupBy(p => p.Motoboy != null ? p.Motoboy.Nome : "Desconhecido")
                .Select(g => new FaturamentoPorMotoboyResumoDTO
                {
                    NomeMotoboy = g.Key,
                    TotalFrete = g.Sum(p => p.ValorFrete),
                    QuantidadePedidos = g.Count()
                })
                .ToList();
            
            return Result<FaturamentoDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<FaturamentoDTO>.Failure($"Erro ao calcular faturamento: {ex.Message}");
        }
    }
}