using AutoMapper;
using Microsoft.Extensions.Configuration;
using NuvPizza.Application.Common;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Services;
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
    private readonly ICupomRepository _cupomRepository;
    private readonly IClienteService _clienteService;
    
    public PedidoService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository, IMapper mapper, IUnitOfWork uow, IViaCepService viaCepService, IWhatsappService whatsappService, IBairroRepository bairroRepository, IConfiguracaoRepository configuracaoRepository, IEmailService emailService, IPagamentoService pagamentoService, INotificacaoService notificacaoService, ICupomRepository cupomRepository, IClienteService clienteService)
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
        _cupomRepository = cupomRepository;
        _clienteService = clienteService;
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
        
        var cupom =  await _cupomRepository.GetAsync(c => c.Codigo == pedidoRegister.CodigoCupom);
        var pedido =  _mapper.Map<Pedido>(pedidoRegister);
        pedido.DataPedido = DateTime.UtcNow.AddHours(-3);
        pedido.Itens = new List<ItemPedido>();
        pedido.ValorFrete = bairro.ValorFrete;
        pedido.BairroId = bairro.Id;
        pedido.BairroNome = !string.IsNullOrWhiteSpace(pedidoRegister.BairroNome) ? pedidoRegister.BairroNome : bairro.Nome;
        pedido.Cep = !string.IsNullOrWhiteSpace(pedidoRegister.Cep) ? pedidoRegister.Cep : enderecoViaCep.Cep;
        pedido.Logradouro = !string.IsNullOrWhiteSpace(pedidoRegister.Logradouro) ? pedidoRegister.Logradouro : enderecoViaCep.Logradouro;
        pedido.Complemento = pedidoRegister.Complemento;
        pedido.Numero = pedidoRegister.Numero;
        pedido.FormaPagamento = formaPagamentoEnum;
        pedido.CupomId = null;
        if (cupom != null)
        {
            var jaUtilizou = await _pedidoRepository.VerificarCupomUtilizadoAsync(pedidoRegister.TelefoneCliente, cupom.Id);
            if (jaUtilizou)
            {
                return Result<PedidoDTO>.Failure("Você já utilizou este cupom em outro pedido.");
            }
            pedido.CupomId = cupom.Id;
        }

        if (formaPagamentoEnum != FormaPagamento.MercadoPago)
        {
            pedido.StatusPedido = StatusPedido.Confirmado; 
        }
        
        foreach (var itemDto in pedidoRegister.Itens)
        {
            var produto = await _produtoRepository.GetAsync(p => p.Id == itemDto.ProdutoId);
            if (produto is null) { return Result<PedidoDTO>.Failure("Produto não encontrado"); }
            if (itemDto.Quantidade <= 0) { return Result<PedidoDTO>.Failure("Quantidade precisa ser maior que 0"); }

            var precoFinal = produto.PrecoPromocional.HasValue && produto.PrecoPromocional.Value > 0 
                ? produto.PrecoPromocional.Value 
                : produto.Preco;
            var nomeFinal = produto.Nome;

            // Se for meio a meio
            if (itemDto.ProdutoSecundarioId.HasValue)
            {
                var produtoSecundario = await _produtoRepository.GetAsync(p => p.Id == itemDto.ProdutoSecundarioId.Value);
                if (produtoSecundario != null)
                {
                    nomeFinal = $"{produto.Nome} / {produtoSecundario.Nome}";
                    
                    var precoSabor1 = produto.PrecoPromocional.HasValue && produto.PrecoPromocional.Value > 0 
                        ? produto.PrecoPromocional.Value 
                        : produto.Preco;
                        
                    var precoSabor2 = produtoSecundario.PrecoPromocional.HasValue && produtoSecundario.PrecoPromocional.Value > 0 
                        ? produtoSecundario.PrecoPromocional.Value 
                        : produtoSecundario.Preco;

                    // Regra: Soma dos dois sabores (promocionais se houver) dividido por 2
                    precoFinal = (precoSabor1 + precoSabor2) / 2;
                }
            }

            // Se tiver borda
            if (itemDto.BordaId.HasValue)
            {
                var borda = await _produtoRepository.GetAsync(p => p.Id == itemDto.BordaId.Value);
                if (borda != null)
                {
                    nomeFinal += $" (Borda: {borda.Nome})";
                    // Borda usually doesn't have promotional price, but we use the regular one as before
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
                        
                        // Preço efetivo do produto escolhido (promocional ou normal)
                        var precoEscolhido = produtoEscolhido.PrecoPromocional.HasValue && produtoEscolhido.PrecoPromocional.Value > 0
                            ? produtoEscolhido.PrecoPromocional.Value
                            : produtoEscolhido.Preco;

                        // Se a escolha em si for meio a meio
                        if (escolhaDto.ProdutoSecundarioId.HasValue)
                        {
                            var escolhaSecundaria = await _produtoRepository.GetAsync(p => p.Id == escolhaDto.ProdutoSecundarioId.Value);
                            if (escolhaSecundaria != null)
                            {
                                nomeEscolha = $"{produtoEscolhido.Nome} / {escolhaSecundaria.Nome}";
                                
                                // Meio a meio: média dos dois sabores
                                var precoSecundario = escolhaSecundaria.PrecoPromocional.HasValue && escolhaSecundaria.PrecoPromocional.Value > 0
                                    ? escolhaSecundaria.PrecoPromocional.Value
                                    : escolhaSecundaria.Preco;
                                precoEscolhido = (precoEscolhido + precoSecundario) / 2;
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

                        // --- LÓGICA DE ALLOWANCE (ValorCobertura) ---
                        // Busca o template correspondente na lista do produto combo (já carregada via Include)
                        var template = produto.ComboTemplates?.FirstOrDefault(t => t.Id == escolhaDto.ComboItemTemplateId);
                        if (template != null && template.ValorCobertura > 0)
                        {
                            var valorExtra = Math.Max(0, precoEscolhido - template.ValorCobertura);
                            if (valorExtra > 0)
                            {
                                item.PrecoUnitario += valorExtra;
                                nomeEscolha += $" [+{valorExtra.ToString("C2", new System.Globalization.CultureInfo("pt-BR"))}]";
                            }
                        }
                        // --- FIM LÓGICA DE ALLOWANCE ---

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

        var totalItensBruto = pedido.Itens.Sum(i => i.Total);

        // 1. Validação do Pedido Mínimo Global (R$20)
        if (totalItensBruto < 20)
        {
            return Result<PedidoDTO>.Failure("O valor mínimo para pedidos é de R$ 20,00 (subtotal).");
        }

        // 2. Validação do Pedido Mínimo por Cupom
        if (cupom != null && cupom.PedidoMinimo > 0 && totalItensBruto < cupom.PedidoMinimo)
        {
            return Result<PedidoDTO>.Failure($"Este cupom exige um pedido mínimo de {cupom.PedidoMinimo:C2}.");
        }

        var valorDescontoPercentual = cupom != null ? cupom.DescontoPorcentagem : 0;
        
        // Save the absolute discount value in currency
        pedido.ValorDesconto = (totalItensBruto / 100M) * valorDescontoPercentual;
        
        if (cupom != null && cupom.FreteGratis) pedido.ValorFrete = 0;
        
        pedido.ValorTotal = totalItensBruto - pedido.ValorDesconto + pedido.ValorFrete;
        
        _pedidoRepository.Create(pedido);
        await _uow.CommitAsync();

        // CRM: Garante que o cliente exista no banco para associar ao pedido
        try
        {
            var clienteId = await _clienteService.EnsureClienteExistsAsync(
                pedido.TelefoneCliente, pedido.NomeCliente, pedido.EmailCliente);
            pedido.ClienteId = clienteId;
            _pedidoRepository.Update(pedido);
            await _uow.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRM] Erro ao vincular cliente: {ex.Message}");
        }
        
        var linkWpp = _whatsappService.GerarLinkPedido(pedido);
        
        var pedidoDto = _mapper.Map<PedidoDTO>(pedido);
        pedidoDto.LinkWhatsapp = linkWpp;
        
        if (formaPagamentoEnum != FormaPagamento.MercadoPago)
        {
            await _notificacaoService.NotificarNovoPedido(pedidoDto);

            // Dispara the email in background (Fire-and-Forget) para não travar a tela de Checkout do cliente
            if (!string.IsNullOrEmpty(pedido.EmailCliente))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.EnviarEmailConfirmacao(pedido);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Email FireAndForget Error] {ex.Message}");
                    }
                });
            }
        }

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

        bool eraEntregue = pedido.StatusPedido == Domain.Enums.StatusPedido.Entrega;

        pedido.StatusPedido = newStatus.StatusDoPedido;
        await _uow.CommitAsync();

        // CRM: Adiciona os pontos/gasto ao cliente apenas quando o pedido é finalizado/entregue
        if (!eraEntregue && pedido.StatusPedido == Domain.Enums.StatusPedido.Entrega && pedido.ClienteId.HasValue)
        {
            try
            {
                await _clienteService.AddPedidoToRankingAsync(pedido.ClienteId.Value, pedido.ValorTotal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRM] Erro ao adicionar pontos no ranking: {ex.Message}");
            }
        }
        
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
            pedidoDto.LinkWhatsapp = _whatsappService.GerarLinkPedido(pedido);
            
            // Dispara para o painel como um NOVO pedido apenas agora que foi pago
            await _notificacaoService.NotificarNovoPedido(pedidoDto);
            
            // Dispara the email de confirmação ao cliente
            if (!string.IsNullOrEmpty(pedido.EmailCliente))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.EnviarEmailConfirmacao(pedido);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Email FireAndForget Error] {ex.Message}");
                    }
                });
            }

            return Result<PedidoDTO>.Success(pedidoDto);
        }
        catch (Exception ex)
        {
            return Result<PedidoDTO>.Failure($"Erro ao salvar pagamento: {ex.Message}");
        }
    }

    public async Task<Result<UltimoEnderecoDTO>> GetUltimoEnderecoPorTelefoneAsync(string telefone)
    {
        var ultimoPedido = await _pedidoRepository.GetUltimoPedidoPorTelefoneAsync(telefone);

        if (ultimoPedido == null)
            return Result<UltimoEnderecoDTO>.Failure("Nenhum pedido anterior encontrado para este número.");

        var dto = new UltimoEnderecoDTO
        {
            NomeCliente = ultimoPedido.NomeCliente,
            TelefoneCliente = ultimoPedido.TelefoneCliente,
            EmailCliente = ultimoPedido.EmailCliente,
            Logradouro = ultimoPedido.Logradouro,
            Numero = ultimoPedido.Numero,
            BairroNome = ultimoPedido.BairroNome,
            PontoReferencia = ultimoPedido.PontoReferencia,
            Cep = ultimoPedido.Cep
        };

        return Result<UltimoEnderecoDTO>.Success(dto);
    }

    private async Task<bool> LojaEstaAberta()
    {
        var config = await _configuracaoRepository.GetAsync(c => c.Id == 1);

        if (!config.EstaAberta) return false;

        if (config.DataHoraFechamentoAtual.HasValue && DateTime.UtcNow.AddHours(-3) > config.DataHoraFechamentoAtual.Value)
        {
            return false;
        }

        return true;
    }
}