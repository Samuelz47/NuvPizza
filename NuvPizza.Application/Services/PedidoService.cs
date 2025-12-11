using AutoMapper;
using Microsoft.Extensions.Configuration;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace NuvPizza.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ViaCepService _viaCepService;
    private readonly IConfiguration _configuration;
    
    public PedidoService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository, IMapper mapper, IUnitOfWork uow, ViaCepService viaCepService, IConfiguration configuration)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _mapper = mapper;
        _uow = uow;
        _viaCepService = viaCepService;
        _configuration = configuration;
    }

    public async Task<Result<PedidoDTO>> CreatePedidoAsync(PedidoForRegistrationDTO pedidoRegister)
    {
        var enderecoViaCep = await _viaCepService.CheckAsync(pedidoRegister.Cep);
        if (enderecoViaCep is null) return Result<PedidoDTO>.Failure("CEP Inválido ou não encontrado");
        
        var bairrosPermitios = _configuration.GetSection("ConfiguracoesEntrega:BairrosPermitidos").Get<List<string>>();

        if (!bairrosPermitios.Any(b => b.Equals(enderecoViaCep.Bairro, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<PedidoDTO>.Failure("Desculpe não entregamos nesse bairro");
        }
        
        var pedido =  _mapper.Map<Pedido>(pedidoRegister);
        pedido.DataPedido = DateTime.UtcNow;
        pedido.Itens = new List<ItemPedido>();
        pedido.Bairro = enderecoViaCep.Bairro;
        pedido.Cep = enderecoViaCep.Cep;
        pedido.Logradouro = enderecoViaCep.Logradouro;
        pedido.Complemento = pedidoRegister.Complemento;
        pedido.Numero = pedidoRegister.Numero;

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