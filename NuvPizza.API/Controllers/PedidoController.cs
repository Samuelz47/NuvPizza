using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.DTOs;
using NuvPizza.Application.Interfaces;
using NuvPizza.Application.Services;

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

    [HttpPost]
    public async Task<ActionResult<PedidoDTO>> Create([FromBody] PedidoForRegistrationDTO PedidoDto)
    {
        var pedidoCreated = await _pedidoService.CreateProdutoAsync(PedidoDto);

        if (!pedidoCreated.IsSuccess)
        {
            return BadRequest(new { error = pedidoCreated.Message });
        }
        
        return Created($"/api/pedidos/{pedidoCreated.Data.Id}", pedidoCreated.Data);
    }
}