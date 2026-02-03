using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
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
            
            _logger.LogInformation($"🔍 Debug Webhook: ID={id}, Topic/Type={topic}");
            
            if (string.IsNullOrEmpty(id)) 
            {
                _logger.LogWarning("⚠️ Webhook ignorado: ID veio nulo.");
                return Ok();
            }
            
            if (topic != "payment")
            {
                _logger.LogInformation($"ℹ️ Webhook ignorado: Tópico '{topic}' não é pagamento.");
                return Ok();
            }

            _logger.LogInformation($"🔔 Webhook: Processando Pagamento ID: {id}");
            
            var consulta = await _pagamentoService.ConsultarStatusPagamentoAsync(id);
            if (!consulta.IsSuccess)
            {
                _logger.LogError($"❌ Webhook: Erro ao consultar MP: {consulta.Message}");
                return Ok();
            }
            
            var dadosPagamento = consulta.Data;
            _logger.LogInformation($"🔔 Webhook: Status do Pagamento: {dadosPagamento.Status}");

            if (dadosPagamento.Status == "approved")
            {
                if (Guid.TryParse(dadosPagamento.PedidoIdExterno, out Guid pedidoId))
                {
                    _logger.LogInformation($"🔔 Webhook: Tentando atualizar Pedido {pedidoId} para EmPreparo...");

                    var updateDto = new StatusPedidoForUpdateDTO { StatusDoPedido = StatusPedido.Confirmado };
                    
                    // AQUI ESTÁ A MUDANÇA: Pegamos o resultado!
                    var resultado = await _pedidoService.UpdateStatusPedidoAsync(pedidoId, updateDto);

                    await _notificacaoService.NotificarAtualizacaoStatus(pedidoId, (int)StatusPedido.Confirmado);
                    
                    if (resultado.IsSuccess)
                    {
                        _logger.LogInformation($"✅ SUCESSO: Pedido {pedidoId} atualizado e notificado!");
                        // Não chamamos _notificacaoService aqui, pois o PedidoService já chamou!
                    }
                    else
                    {
                        _logger.LogError($"❌ FALHA: Pedido {pedidoId} não atualizou. Motivo: {resultado.Message}");
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