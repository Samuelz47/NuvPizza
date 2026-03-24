using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Services;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Services;
using NuvPizza.Domain.Interfaces;

namespace NuvPizza.Tests;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepoMock;
    private readonly Mock<IProdutoRepository> _produtoRepoMock;
    private readonly Mock<IBairroRepository> _bairroRepoMock;
    private readonly Mock<IConfiguracaoRepository> _configRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IViaCepService> _viaCepMock;
    private readonly Mock<IWhatsappService> _whatsappMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IPagamentoService> _pagamentoMock;
    private readonly Mock<INotificacaoService> _notificacaoMock;
    private readonly Mock<ICupomRepository> _cupomRepoMock;
    private readonly Mock<IClienteService> _clienteServiceMock;
    
    private readonly PedidoService _sut;
    
    public PedidoServiceTests()
    {
        _pedidoRepoMock = new Mock<IPedidoRepository>();
        _produtoRepoMock = new Mock<IProdutoRepository>();
        _bairroRepoMock = new Mock<IBairroRepository>();
        _configRepoMock = new Mock<IConfiguracaoRepository>();
        _mapperMock = new Mock<IMapper>();
        _uowMock = new Mock<IUnitOfWork>();
        _viaCepMock = new Mock<IViaCepService>();
        _whatsappMock = new Mock<IWhatsappService>();
        _emailMock = new Mock<IEmailService>();
        _pagamentoMock = new Mock<IPagamentoService>();
        _notificacaoMock = new Mock<INotificacaoService>();
        _cupomRepoMock = new Mock<ICupomRepository>();
        _clienteServiceMock = new Mock<IClienteService>();

        _sut = new PedidoService(
            _pedidoRepoMock.Object,
            _produtoRepoMock.Object,
            _mapperMock.Object,
            _uowMock.Object,
            _viaCepMock.Object,
            _whatsappMock.Object,
            _bairroRepoMock.Object,
            _configRepoMock.Object,
            _emailMock.Object,
            _pagamentoMock.Object,
            _notificacaoMock.Object,
            _cupomRepoMock.Object,
            _clienteServiceMock.Object
        );
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveRetornarFalha_QuandoLojaEstiverFechada()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO { FormaPagamento = "Pix" };
    
        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Configuracao, bool>>>()))
            .ReturnsAsync(new Configuracao { EstaAberta = false });

        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("fechada"); // Verifica parte da mensagem de erro
    
        _pedidoRepoMock.Verify(x => x.Create(It.IsAny<Pedido>()), Times.Never);
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveRetornarFalha_QuandoCepForInvalido()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO { Cep = "00000-000", FormaPagamento = "Pix" };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Configuracao, bool>>>()))
            .ReturnsAsync(new Configuracao { EstaAberta = true });

        _viaCepMock.Setup(x => x.CheckAsync(pedidoDto.Cep))
            .ReturnsAsync((ViaCepResponse?)null);

        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("CEP Inválido");
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveCriarPedido_QuandoDadosForemValidos()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO 
        { 
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO> 
            { 
                new ItemPedidoForRegistrationDTO { ProdutoId = 1, Quantidade = 2 } 
            } 
        };

        _configRepoMock.Setup(c => c.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Configuracao, bool>>>()))
            .ReturnsAsync(new Configuracao { EstaAberta = true });

        var viaCepResponse = new ViaCepResponse { Bairro = "Centro", Logradouro = "Rua Teste", Cep = "59000-000" };
        _viaCepMock.Setup(v => v.CheckAsync(pedidoDto.Cep)).ReturnsAsync(viaCepResponse);

        var bairro = new Bairro { Id = 1, Nome = "Centro", ValorFrete = 10.0m };
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Bairro, bool>>>()))
            .ReturnsAsync(bairro);

        var produto = new Produto { Id = 1, Nome = "Pizza Teste", Preco = 50.0m };
        _produtoRepoMock.Setup(p => p.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Produto, bool>>>()))
            .ReturnsAsync(produto);

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _pedidoRepoMock.Verify(x => x.Create(It.IsAny<Pedido>()), Times.Once);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveFalhar_QuandoBairroNaoForAtendido()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO { Cep = "59000-000", FormaPagamento = "Pix" };
        
        _configRepoMock.Setup(c => c.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>()))
            .ReturnsAsync(new Configuracao { EstaAberta = true });
        
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>()))
            .ReturnsAsync(new ViaCepResponse { Bairro = "Alecrim", Logradouro = "Rua X" });

        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>()))
            .ReturnsAsync((Bairro?)null);

        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("não entregamos");
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveFalhar_QuandoProdutoNaoExistir()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO { ProdutoId = 999, Quantidade = 1 } // ID 999 não existe
            }
        };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>()))
            .ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>()))
            .ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>()))
            .ReturnsAsync(new Bairro { Nome = "Centro" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.IsAny<Expression<Func<Produto, bool>>>()))
            .ReturnsAsync((Produto?)null);

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("não encontrado");
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveFalhar_QuandoQuantidadeForZeroOuNegativa()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO { ProdutoId = 1, Quantidade = 0 } // ERRO AQUI
            }
        };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { Nome = "Centro" });
        _produtoRepoMock.Setup(p => p.GetAsync(It.IsAny<Expression<Func<Produto, bool>>>())).ReturnsAsync(new Produto { Id = 1 });

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        // Act
        var result = await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("maior que 0");
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveCalcularValorTotalCorretamente()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO { ProdutoId = 1, Quantidade = 2 }
            }
        };

        decimal precoProduto = 50.00m;
        decimal valorFrete = 10.00m;
        decimal totalEsperado = (precoProduto * 2) + valorFrete; // 110.00
        
        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>()))
            .ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.IsAny<Expression<Func<Produto, bool>>>()))
            .ReturnsAsync(new Produto { Id = 1, Preco = precoProduto, Nome = "Pizza" });

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        // Act
        await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p => 
            p.ValorTotal == totalEsperado && // Valida se o total gravado é 110.00
            p.ValorFrete == valorFrete
        )), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveCalcularValorMeioAMeioCorretamente()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO 
                { 
                    ProdutoId = 1, // Sabor 1
                    ProdutoSecundarioId = 2, // Sabor 2
                    Quantidade = 1 
                }
            }
        };

        decimal precoSabor1 = 50.00m;
        decimal precoSabor2 = 60.00m;
        decimal valorFrete = 10.00m;
        decimal precoMeioAMeio = (precoSabor1 + precoSabor2) / 2; // 55.00
        decimal totalEsperado = precoMeioAMeio + valorFrete; // 65.00

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = 1 }))))
            .ReturnsAsync(new Produto { Id = 1, Preco = precoSabor1, Nome = "Sabor 1" });
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = 2 }))))
            .ReturnsAsync(new Produto { Id = 2, Preco = precoSabor2, Nome = "Sabor 2" });

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        // Act
        await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p => 
            p.ValorTotal == totalEsperado &&
            p.Itens.First().PrecoUnitario == precoMeioAMeio
        )), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveAdicionarPrecoBordaCorretamente()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO 
                { 
                    ProdutoId = 1, 
                    BordaId = 3, 
                    Quantidade = 1 
                }
            }
        };

        decimal precoProduto = 50.00m;
        decimal precoBorda = 15.00m;
        decimal valorFrete = 10.00m;
        decimal totalEsperado = precoProduto + precoBorda + valorFrete; // 75.00

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = 1 }))))
            .ReturnsAsync(new Produto { Id = 1, Preco = precoProduto, Nome = "Pizza" });
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = 3 }))))
            .ReturnsAsync(new Produto { Id = 3, Preco = precoBorda, Nome = "Borda Catupiry" });

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        // Act
        await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p => 
            p.ValorTotal == totalEsperado &&
            p.Itens.First().PrecoUnitario == (precoProduto + precoBorda)
        )), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveProcessarComboComEscolhasCorretamente()
    {
        // Arrange
        var comboId = 10;
        var escolha1ProdutoId = 11;
        var escolha2ProdutoId = 12;
        var precoCombo = 100.00m;
        var valorFrete = 10.00m;

        var pedidoDto = new PedidoForRegistrationDTO
        {
            Cep = "59000-000",
            FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO>
            {
                new ItemPedidoForRegistrationDTO 
                { 
                    ProdutoId = comboId, 
                    Quantidade = 1,
                    EscolhasCombo = new List<ItemPedidoComboEscolhaForRegistrationDTO>
                    {
                        new ItemPedidoComboEscolhaForRegistrationDTO { ProdutoEscolhidoId = escolha1ProdutoId, ComboItemTemplateId = 1 },
                        new ItemPedidoComboEscolhaForRegistrationDTO { ProdutoEscolhidoId = escolha2ProdutoId, ComboItemTemplateId = 2 }
                    }
                }
            }
        };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = comboId }))))
            .ReturnsAsync(new Produto { Id = comboId, Preco = precoCombo, Nome = "Combo", Categoria = Domain.Enums.Categoria.Combo });
        
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = escolha1ProdutoId }))))
            .ReturnsAsync(new Produto { Id = escolha1ProdutoId, Nome = "Escolha 1" });

        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = escolha2ProdutoId }))))
            .ReturnsAsync(new Produto { Id = escolha2ProdutoId, Nome = "Escolha 2" });

        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        // Act
        await _sut.CreatePedidoAsync(pedidoDto);

        // Assert
        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p => 
            p.ValorTotal == (precoCombo + valorFrete) &&
            p.Itens.First().EscolhasCombo.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_DeveAdicionarValorExtra_QuandoEscolhaExcederValorCobertura()
    {
        var comboId = 20; var pizzaId = 21; var templateId = 5;
        var precoCombo = 100.00m; var precoPizza = 75.00m; var valorCobertura = 60.00m;
        var valorExtraEsperado = 15.00m; var valorFrete = 10.00m;
        var totalEsperado = precoCombo + valorExtraEsperado + valorFrete;

        var pedidoDto = new PedidoForRegistrationDTO { Cep = "59000-000", FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO> { new ItemPedidoForRegistrationDTO { ProdutoId = comboId, Quantidade = 1,
                EscolhasCombo = new List<ItemPedidoComboEscolhaForRegistrationDTO> { new ItemPedidoComboEscolhaForRegistrationDTO { ProdutoEscolhidoId = pizzaId, ComboItemTemplateId = templateId } } } } };

        var comboProduto = new Produto { Id = comboId, Preco = precoCombo, Nome = "Combo Casal", Categoria = Domain.Enums.Categoria.Combo,
            ComboTemplates = new List<ComboItemTemplate> { new ComboItemTemplate { Id = templateId, ValorCobertura = valorCobertura } } };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = comboId })))).ReturnsAsync(comboProduto);
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = pizzaId })))).ReturnsAsync(new Produto { Id = pizzaId, Preco = precoPizza, Nome = "Pizza Cama\u00e3o" });
        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        await _sut.CreatePedidoAsync(pedidoDto);

        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p =>
            p.ValorTotal == totalEsperado && p.Itens.First().PrecoUnitario == (precoCombo + valorExtraEsperado)
        )), Times.Once);
    }

    [Fact]
    public async Task CreatePedidoAsync_NaoDeveAdicionarValorExtra_QuandoEscolhaEstiverDentroDoValorCobertura()
    {
        var comboId = 30; var pizzaId = 31; var templateId = 6;
        var precoCombo = 100.00m; var precoPizza = 55.00m; var valorCobertura = 60.00m;
        var valorFrete = 10.00m; var totalEsperado = precoCombo + valorFrete;

        var pedidoDto = new PedidoForRegistrationDTO { Cep = "59000-000", FormaPagamento = "Pix",
            Itens = new List<ItemPedidoForRegistrationDTO> { new ItemPedidoForRegistrationDTO { ProdutoId = comboId, Quantidade = 1,
                EscolhasCombo = new List<ItemPedidoComboEscolhaForRegistrationDTO> { new ItemPedidoComboEscolhaForRegistrationDTO { ProdutoEscolhidoId = pizzaId, ComboItemTemplateId = templateId } } } } };

        var comboProduto = new Produto { Id = comboId, Preco = precoCombo, Nome = "Combo Casal", Categoria = Domain.Enums.Categoria.Combo,
            ComboTemplates = new List<ComboItemTemplate> { new ComboItemTemplate { Id = templateId, ValorCobertura = valorCobertura } } };

        _configRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Configuracao, bool>>>())).ReturnsAsync(new Configuracao { EstaAberta = true });
        _viaCepMock.Setup(v => v.CheckAsync(It.IsAny<string>())).ReturnsAsync(new ViaCepResponse { Bairro = "Centro" });
        _bairroRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<Bairro, bool>>>())).ReturnsAsync(new Bairro { ValorFrete = valorFrete, Nome = "Centro" });
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = comboId })))).ReturnsAsync(comboProduto);
        _produtoRepoMock.Setup(p => p.GetAsync(It.Is<Expression<Func<Produto, bool>>>(e => e.Compile()(new Produto { Id = pizzaId })))).ReturnsAsync(new Produto { Id = pizzaId, Preco = precoPizza, Nome = "Pizza Calabresa" });
        _mapperMock.Setup(m => m.Map<Pedido>(pedidoDto)).Returns(new Pedido());
        _mapperMock.Setup(m => m.Map<PedidoDTO>(It.IsAny<Pedido>())).Returns(new PedidoDTO());

        await _sut.CreatePedidoAsync(pedidoDto);

        _pedidoRepoMock.Verify(x => x.Create(It.Is<Pedido>(p =>
            p.ValorTotal == totalEsperado && p.Itens.First().PrecoUnitario == precoCombo
        )), Times.Once);
    }
}