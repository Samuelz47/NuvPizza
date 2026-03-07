using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Enums;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]
public class PagamentoController : ControllerBase
{
    private readonly IPagamentoService _pagamentoService;
    private readonly IPedidoService _pedidoService;
    private readonly ILogger<PagamentoController> _logger;
    private readonly INotificacaoService _notificacaoService;

    public PagamentoController(IPagamentoService pagamentoService, ILogger<PagamentoController> logger, IPedidoService pedidoService, INotificacaoService notificacaoService)
    {
        _pagamentoService = pagamentoService;
        _logger = logger;
        _pedidoService = pedidoService;
        _notificacaoService = notificacaoService;
    }

    [HttpPost("criar-link")]
    public async Task<IActionResult> CriarLinkPagamento([FromBody] CriarPreferenceDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _pagamentoService.CriarPreferenciaAsync(dto);

        if (result.IsSuccess)
        {
            return Ok(new { url = result.Data });
        }

        return BadRequest(result.Message);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        try
        {
            // Log para provar que o MP bateu na porta
            _logger.LogInformation("🔔 Webhook: Recebendo notificação...");

            string topic = Request.Query["type"];
            string id = Request.Query["data.id"];

            if (string.IsNullOrEmpty(id))
            {
                id = Request.Query["id"];
                topic = Request.Query["topic"];
            }
            
            if (string.IsNullOrEmpty(id) || topic != "payment") return Ok();

            // 1. Consulta o MP
            var consulta = await _pagamentoService.ConsultarStatusPagamentoAsync(id);
            if (!consulta.IsSuccess) return Ok();
            
            var dadosPagamento = consulta.Data;

            // 2. Se aprovado, processa a atualização
            if (dadosPagamento.Status == "approved")
            {
                if (Guid.TryParse(dadosPagamento.PedidoIdExterno, out Guid pedidoId))
                {
                    _logger.LogInformation($"🔔 Webhook: Pagamento aprovado para Pedido {pedidoId}. Tipo: {dadosPagamento.TipoPagamento}");

                    // 3. Força o Enum MercadoPago para que o Painel exiba "Online (MP)"
                    // e o entregador saiba que não precisa cobrar na entrega.
                    FormaPagamento formaReal = FormaPagamento.MercadoPago;

                    // 4. Chama o novo método do Service que atualiza Status E Pagamento
                    var resultado = await _pedidoService.ConfirmarPagamentoAsync(pedidoId, formaReal);

                    if (resultado.IsSuccess)
                    {
                        _logger.LogInformation($"✅ Pedido atualizado para {formaReal} e Confirmado! (SignalR disparado do Service)");
                    }
                }
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"🔥 Webhook Exception: {ex.Message}");
            return StatusCode(500);
        }
    }
}