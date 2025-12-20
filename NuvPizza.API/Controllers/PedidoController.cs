using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Services;
using NuvPizza.Domain.Pagination;
using System.Text.Json;

namespace NuvPizza.API.Controllers;
[Route("[controller]")]
[ApiController]

public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;

    public PedidoController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpGet]
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
        var pedidoCreated = await _pedidoService.CreatePedidoAsync(PedidoDto);

        if (!pedidoCreated.IsSuccess)
        {
            return BadRequest(new { error = pedidoCreated.Message });
        }
        
        return Created($"/api/pedidos/{pedidoCreated.Data.Id}", pedidoCreated.Data);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<PedidoDTO>> UpdateStatus(Guid id, StatusPedidoForUpdateDTO newStatus)
    {
        var pedidoUpdated = await _pedidoService.UpdateStatusPedidoAsync(id, newStatus);
        
        if (!pedidoUpdated.IsSuccess) return BadRequest(pedidoUpdated.Message);
            
        return Ok(pedidoUpdated.Data);
    }
}