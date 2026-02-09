using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Services;
using NuvPizza.Domain.Pagination;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace NuvPizza.API.Controllers;
[Route("[controller]")]
[ApiController]

public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;
    private readonly IPagamentoService _pagamentoService;

    public PedidoController(IPedidoService pedidoService, IPagamentoService pagamentoService)
    {
        _pedidoService = pedidoService;
        _pagamentoService = pagamentoService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllPedidos([FromQuery]PedidoParameters pedidoParameters)
    {
        var pagedResult = await _pedidoService.GetAllPedidosAsync(pedidoParameters);

        var paginationMetadata = new
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.TotalPages,
            pagedResult.PageNumber,
            pagedResult.Items,
            pagedResult.HasNextPage,
            pagedResult.HasPreviousPage,
        };
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
        
        return Ok(paginationMetadata.Items);
    }

    [HttpPost]
    public async Task<ActionResult<PedidoDTO>> Create([FromBody] PedidoForRegistrationDTO PedidoDto)
    {
        var pedidoResult = await _pedidoService.CreatePedidoAsync(PedidoDto);

        if (!pedidoResult.IsSuccess)
        {
            return BadRequest(new { error = pedidoResult.Message });
        }
        
        var pedidoCriado = pedidoResult.Data;
        var preferenceDto = new CriarPreferenceDTO
        {
            Titulo = $"Pedido NuvPizza #{pedidoCriado.Numero}", // Ou use pedidoCriado.Id se não tiver Numero
            Quantidade = 1,
            PrecoUnitario = pedidoCriado.ValorTotal,
            ExternalReference = pedidoCriado.Id.ToString(),
            EmailPagador = PedidoDto.EmailCliente
        };
        
        var linkResult = await _pagamentoService.CriarPreferenciaAsync(preferenceDto);

        if (!linkResult.IsSuccess)
        {
            return Created($"/pedidos/{pedidoCriado.Id}", new
            {
                pedido = pedidoCriado,
                warning = "Pedido Salvo, mas houve erro ao gerar link",
                error = linkResult.Message
            });
        }

        return Created($"/pedidos/{pedidoCriado.Id}", new
        {
            pedido = pedidoCriado,
            paymentLink = linkResult.Data
        });
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult<PedidoDTO>> UpdateStatus(Guid id, StatusPedidoForUpdateDTO newStatus)
    {
        var pedidoUpdated = await _pedidoService.UpdateStatusPedidoAsync(id, newStatus);
        
        if (!pedidoUpdated.IsSuccess) return BadRequest(pedidoUpdated.Message);
            
        return Ok(pedidoUpdated.Data);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _pedidoService.GetPedidoByIdAsync(id);

        if (!result.IsSuccess) return NotFound(result.Message);

        return Ok(result.Data);
    }
}