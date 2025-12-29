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
            _notificacaoMock.Object
        );
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveRetornarFalha_QuandoLojaEstiverFechada()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO();
    
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
        var pedidoDto = new PedidoForRegistrationDTO { Cep = "00000-000" };

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

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreatePedidoAsync_DeveFalhar_QuandoBairroNaoForAtendido()
    {
        // Arrange
        var pedidoDto = new PedidoForRegistrationDTO { Cep = "59000-000" };
        
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
}