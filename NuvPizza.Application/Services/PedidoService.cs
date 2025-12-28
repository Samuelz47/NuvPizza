using AutoMapper;
using Microsoft.Extensions.Configuration;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NuvPizza.Domain.Enums;
using NuvPizza.Domain.Pagination;

namespace NuvPizza.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IBairroRepository _bairroRepository;
    private readonly IConfiguracaoRepository _configuracaoRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;
    private readonly ViaCepService _viaCepService;
    private readonly IWhatsappService _whatsappService;
    private readonly IEmailService _emailService;
    private readonly IPagamentoService _pagamentoService;
    private readonly INotificacaoService _notificacaoService;
    
    public PedidoService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository, IMapper mapper, IUnitOfWork uow, ViaCepService viaCepService, IWhatsappService whatsappService, IBairroRepository bairroRepository, IConfiguracaoRepository configuracaoRepository, IEmailService emailService, IPagamentoService pagamentoService, INotificacaoService notificacaoService)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _mapper = mapper;
        _uow = uow;
        _viaCepService = viaCepService;
        _whatsappService = whatsappService;
        _bairroRepository = bairroRepository;
        _configuracaoRepository = configuracaoRepository;
        _emailService = emailService;
        _pagamentoService = pagamentoService;
        _notificacaoService = notificacaoService;
    }

    public async Task<Result<PedidoDTO>> CreatePedidoAsync(PedidoForRegistrationDTO pedidoRegister)
    {
        if (!await LojaEstaAberta()) return Result<PedidoDTO>.Failure("Desculpe, a pizzaria está fechada no momento");
        
        var enderecoViaCep = await _viaCepService.CheckAsync(pedidoRegister.Cep);
        if (enderecoViaCep is null) return Result<PedidoDTO>.Failure("CEP Inválido ou não encontrado");

        var nomeBairroViaCep = enderecoViaCep.Bairro.Trim().ToLower();
        var bairro = await _bairroRepository.GetAsync(b => b.Nome.ToLower() == nomeBairroViaCep);
        if (bairro is null) return Result<PedidoDTO>.Failure("Desculpe não entregamos nesse bairro");

        var pedido =  _mapper.Map<Pedido>(pedidoRegister);
        pedido.DataPedido = DateTime.Now;
        pedido.Itens = new List<ItemPedido>();
        pedido.ValorFrete = bairro.ValorFrete;
        pedido.BairroId = bairro.Id;
        pedido.BairroNome = bairro.Nome;
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
                PrecoUnitario = produto.Preco
            };
            
            pedido.Itens.Add(item);
        }
        
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Total) + pedido.ValorFrete;
        _pedidoRepository.Create(pedido);
        await _uow.CommitAsync();
        
        var linkWpp = _whatsappService.GerarLinkPedido(pedido);
        
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        pedidoDto.LinkWhatsapp = linkWpp;
        return Result<PedidoDTO>.Success(pedidoDto);
    }

    public async Task<Result<PedidoDTO>> UpdateStatusPedidoAsync(Guid Id, StatusPedidoForUpdateDTO newStatus)
    {
        var pedido = await _pedidoRepository.GetAsync(p => p.Id == Id);
        
        if (pedido is null) return Result<PedidoDTO>.Failure("Pedido não encontrado");

        if (pedido.StatusPedido == StatusPedido.Cancelado)
            return Result<PedidoDTO>.Failure("Pedido Cancelado, necessário refazer");
        if (newStatus.StatusDoPedido == StatusPedido.Cancelado)
        {
            if (pedido.StatusPedido == StatusPedido.Entrega)
            {
                return Result<PedidoDTO>.Failure("Não é possível cancelar um pedido que já foi entregue.");
            }
        }
        else
        {
            int statusAtualNumerico = (int)pedido.StatusPedido;
            int statusNovoNumerico = (int)newStatus.StatusDoPedido;
            int proximoStatusEsperado = statusAtualNumerico + 1;

            if (statusNovoNumerico != proximoStatusEsperado)
            {
                return Result<PedidoDTO>.Failure($"Fluxo inválido. O status atual é '{pedido.StatusPedido}', o próximo passo obrigatório é '{(StatusPedido)proximoStatusEsperado}'.");
            }
        }

        pedido.StatusPedido = newStatus.StatusDoPedido;
        await _uow.CommitAsync();
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        return Result<PedidoDTO>.Success(pedidoDto);
    }

    public async Task<PagedResult<PedidoDTO>> GetAllPedidosAsync(PedidoParameters pedidoParameters)
    {
        var pagedResult = await _pedidoRepository.GetAllWithDetailsAsync(pedidoParameters);

        var pedidosDto = _mapper.Map<IEnumerable<PedidoDTO>>(pagedResult.Items);

        return new PagedResult<PedidoDTO>(
            pedidosDto,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount
        );
    }
    
    private async Task<bool> LojaEstaAberta()
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);

        if (!config.EstaAberta) return false;

        if (config.DataHoraFechamentoAtual.HasValue && DateTime.Now > config.DataHoraFechamentoAtual.Value)
        {
            return false;
        }

        return true;
    }
}