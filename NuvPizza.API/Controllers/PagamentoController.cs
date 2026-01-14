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

    public PagamentoController(IPagamentoService pagamentoService, ILogger<PagamentoController> logger, IPedidoService pedidoService)
    {
        _pagamentoService = pagamentoService;
        _logger = logger;
        _pedidoService = pedidoService;
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
            string topic = Request.Query["topic"];
            string id = Request.Query["id"];

            if (string.IsNullOrEmpty(id))
            {
                using StreamReader streamReader = new StreamReader(Request.Body);
                string body = await streamReader.ReadToEndAsync();
                _logger.LogInformation($"Webhook Body recebido: {body}");
                return Ok();
            }

            if (topic != "payment")
            {
                return Ok();
            }
            _logger.LogInformation($"Notificação de Pagamento Recebida! ID: {id}");
            
            var consulta = await _pagamentoService.ConsultarStatusPagamentoAsync(id);
            
            if (!consulta.IsSuccess)
            {
                _logger.LogError($"Erro ao consultar pagamento {id}: {consulta.Message}");
                return Ok();
            }
            
            var dadosPagamento = consulta.Data;

            if (dadosPagamento.Status == "approved")
            {
                if (Guid.TryParse(dadosPagamento.PedidoIdExterno, out Guid pedidoId))
                {
                    var updateDto = new StatusPedidoForUpdateDTO { StatusDoPedido = StatusPedido.EmPreparo };
                    await _pedidoService.UpdateStatusPedidoAsync(pedidoId, updateDto);
                    _logger.LogInformation($"Pedido atualizado com sucesso!");
                }
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro no Webhook: {ex.Message}");
            return StatusCode(500);
        }
    }
}