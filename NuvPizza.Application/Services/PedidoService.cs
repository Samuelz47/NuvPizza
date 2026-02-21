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
    private readonly IViaCepService _viaCepService;
    private readonly IWhatsappService _whatsappService;
    private readonly IEmailService _emailService;
    private readonly IPagamentoService _pagamentoService;
    private readonly INotificacaoService _notificacaoService;
    
    public PedidoService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository, IMapper mapper, IUnitOfWork uow, IViaCepService viaCepService, IWhatsappService whatsappService, IBairroRepository bairroRepository, IConfiguracaoRepository configuracaoRepository, IEmailService emailService, IPagamentoService pagamentoService, INotificacaoService notificacaoService)
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

        if (!Enum.TryParse<FormaPagamento>(pedidoRegister.FormaPagamento, true, out var formaPagamentoEnum))
        {
            // Se não conseguir converter (ex: veio "Bananas"), define um padrão ou retorna erro
            // Aqui vou mapear manualmente o texto "Pagar na Entrega" para "CartaoEntrega" ou "Dinheiro"
            if (pedidoRegister.FormaPagamento == "Pagar na Entrega") 
                formaPagamentoEnum = FormaPagamento.CartaoEntrega;
            else 
                return Result<PedidoDTO>.Failure("Forma de pagamento inválida");
        }
        
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
        pedido.FormaPagamento = formaPagamentoEnum;
        
        if (formaPagamentoEnum != FormaPagamento.MercadoPago)
        {
            pedido.StatusPedido = StatusPedido.Confirmado; 
        }
        
        foreach (var itemDto in pedidoRegister.Itens)
        {
            var produto = await _produtoRepository.GetAsync(p => p.Id == itemDto.ProdutoId);
            if (produto is null) { return Result<PedidoDTO>.Failure("Produto não encontrado"); }
            if (itemDto.Quantidade <= 0) { return Result<PedidoDTO>.Failure("Quantidade precisa ser maior que 0"); }

            var precoFinal = produto.Preco;
            var nomeFinal = produto.Nome;

            // Se for meio a meio
            if (itemDto.ProdutoSecundarioId.HasValue)
            {
                var produtoSecundario = await _produtoRepository.GetAsync(p => p.Id == itemDto.ProdutoSecundarioId.Value);
                if (produtoSecundario != null)
                {
                    nomeFinal = $"{produto.Nome} / {produtoSecundario.Nome}";
                    // Regra: Soma dos dois sabores dividido por 2
                    precoFinal = (produto.Preco + produtoSecundario.Preco) / 2;
                }
            }

            // Se tiver borda
            if (itemDto.BordaId.HasValue)
            {
                var borda = await _produtoRepository.GetAsync(p => p.Id == itemDto.BordaId.Value);
                if (borda != null)
                {
                    nomeFinal += $" (Borda: {borda.Nome})";
                    precoFinal += borda.Preco;
                }
            }

            var item = new ItemPedido
            {
                ProdutoId = produto.Id,
                Nome = nomeFinal,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = precoFinal,
                EscolhasCombo = new List<ItemPedidoComboEscolha>()
            };

            // Se for combo e tiver escolhas
            if (produto.Categoria == Domain.Enums.Categoria.Combo && itemDto.EscolhasCombo != null && itemDto.EscolhasCombo.Any())
            {
                foreach (var escolhaDto in itemDto.EscolhasCombo)
                {
                    var produtoEscolhido = await _produtoRepository.GetAsync(p => p.Id == escolhaDto.ProdutoEscolhidoId);
                    if (produtoEscolhido != null)
                    {
                        var nomeEscolha = produtoEscolhido.Nome;

                        // Se a escolha em si for meio a meio
                        if (escolhaDto.ProdutoSecundarioId.HasValue)
                        {
                            var escolhaSecundaria = await _produtoRepository.GetAsync(p => p.Id == escolhaDto.ProdutoSecundarioId.Value);
                            if (escolhaSecundaria != null)
                            {
                                nomeEscolha = $"{produtoEscolhido.Nome} / {escolhaSecundaria.Nome}";
                            }
                        }

                        // Se a escolha tiver borda
                        if (escolhaDto.BordaId.HasValue)
                        {
                            var bordaC = await _produtoRepository.GetAsync(p => p.Id == escolhaDto.BordaId.Value);
                            if (bordaC != null)
                            {
                                nomeEscolha += $" (Borda: {bordaC.Nome})";
                                // Add borda price to combo item total base price
                                item.PrecoUnitario += bordaC.Preco;
                            }
                        }

                        // Adiciona a string na descrição do Combo para a nota
                        item.Nome += $" [+ {nomeEscolha}]";

                        // Salva no BD a rastreabilidade
                        item.EscolhasCombo.Add(new ItemPedidoComboEscolha
                        {
                            ComboItemTemplateId = escolhaDto.ComboItemTemplateId,
                            ProdutoEscolhidoId = escolhaDto.ProdutoEscolhidoId,
                            ProdutoSecundarioId = escolhaDto.ProdutoSecundarioId,
                            BordaId = escolhaDto.BordaId
                        });
                    }
                }
            }

            pedido.Itens.Add(item);
        }
        
        pedido.ValorTotal = pedido.Itens.Sum(i => i.Total) + pedido.ValorFrete;
        _pedidoRepository.Create(pedido);
        await _uow.CommitAsync();
        
        var linkWpp = _whatsappService.GerarLinkPedido(pedido);
        
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        pedidoDto.LinkWhatsapp = linkWpp;
        await _notificacaoService.NotificarNovoPedido(pedidoDto);
        return Result<PedidoDTO>.Success(pedidoDto);
    }

    public async Task<Result<PedidoDTO>> UpdateStatusPedidoAsync(Guid Id, StatusPedidoForUpdateDTO newStatus)
    {
        var pedido = await _pedidoRepository.GetAsync(p => p.Id == Id);
        
        if (pedido is null) return Result<PedidoDTO>.Failure("Pedido não encontrado");

        if (pedido.StatusPedido == StatusPedido.Cancelado)
            return Result<PedidoDTO>.Failure("Pedido Cancelado, necessário refazer");

        // --- INÍCIO DA CORREÇÃO ---
        if (newStatus.StatusDoPedido != StatusPedido.Cancelado)
        {
            int statusAtualNumerico = (int)pedido.StatusPedido;
            int statusNovoNumerico = (int)newStatus.StatusDoPedido;
            
            // Lógica Flexível: Permite sequência normal (1->2) OU pular pagamento (1->3)
            bool fluxoValido = false;

            // Regra 1: Sequência normal (ex: 3->4)
            if (statusNovoNumerico == statusAtualNumerico + 1) fluxoValido = true;

            // Regra 2: Pagamento Aprovado (Pula de 'Criado' direto para 'Em Preparo')
            if (pedido.StatusPedido == StatusPedido.Criado && newStatus.StatusDoPedido == StatusPedido.EmPreparo) 
                fluxoValido = true;

            if (!fluxoValido)
            {
                return Result<PedidoDTO>.Failure($"Fluxo inválido. Não é permitido ir de '{pedido.StatusPedido}' para '{newStatus.StatusDoPedido}'.");
            }
        }
        // --- FIM DA CORREÇÃO ---

        pedido.StatusPedido = newStatus.StatusDoPedido;
        await _uow.CommitAsync();
        
        // Agora sim o SignalR vai disparar!
        await _notificacaoService.NotificarAtualizacaoStatus(pedido.Id, (int)pedido.StatusPedido);
        
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

    public async Task<Result<PedidoDTO>> GetPedidoByIdAsync(Guid pedidoId)
    {
        var pedido = await _pedidoRepository.GetByIdWithDetailsAsync(pedidoId);
        
        if (pedido is null) return Result<PedidoDTO>.Failure("Pedido inexistente");
        
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        return Result<PedidoDTO>.Success(pedidoDto);
    }

    public async Task<Result<PedidoDTO>> ConfirmarPagamentoAsync(Guid pedidoId, FormaPagamento formaPagamento)
    {
        var pedido = await _pedidoRepository.GetByIdWithDetailsAsync(pedidoId);
        if (pedido == null) return Result<PedidoDTO>.Failure("Pedido não encontrado");

        // Atualiza os dados
        pedido.StatusPedido = StatusPedido.Confirmado; // Ou Recebido
        pedido.FormaPagamento = formaPagamento;        // Atualiza de 'MercadoPago' para 'Pix/Credito'

        try
        {
            await _uow.CommitAsync();
            var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
            return Result<PedidoDTO>.Success(pedidoDto);
        }
        catch (Exception ex)
        {
            return Result<PedidoDTO>.Failure($"Erro ao salvar pagamento: {ex.Message}");
        }
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