using AutoMapper;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    
    public PedidoService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository, IMapper mapper, IUnitOfWork uow)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<Result<PedidoDTO>> CreateProdutoAsync(PedidoForRegistrationDTO pedidoRegister)
    {
        var pedido =  _mapper.Map<Pedido>(pedidoRegister);
        pedido.DataPedido = DateTime.UtcNow;
        pedido.Itens = new List<ItemPedido>();

        foreach (var itemDto in pedidoRegister.Itens)
        {
            var produto = await _produtoRepository.GetAsync(p => p.Id == itemDto.ProdutoId);
            if (produto is null) { return Result<PedidoDTO>.Failure("Produto não encontrado"); }
            if (itemDto.Quantidade <= 0) { return Result<PedidoDTO>.Failure("Quantidade precisa ser maior que 0"); }

            var item = new ItemPedido
            {
                ProdutoId = produto.Id,
                Nome = produto.Nome,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = produto.Preco,
                PedidoId = pedido.Id
            };
            
            pedido.Itens.Add(item);
        }
        
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Total);
        _pedidoRepository.Create(pedido);
        await _uow.CommitAsync();
        
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        return Result<PedidoDTO>.Success(pedidoDto);
    }
}