using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Pagination;
using System.Text.Json;

namespace NuvPizza.API.Controllers;

[Route("[controller]")]
[ApiController]
public class ClienteController : Controller
{
    private readonly IClienteService _clienteService;

    public ClienteController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [Authorize]
    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking([FromQuery] ClienteRankingParameters parameters)
    {
        var result = await _clienteService.GetRankingAsync(parameters);

        var metadata = new
        {
            result.TotalCount,
            result.PageSize,
            result.PageNumber,
            result.TotalPages,
            result.HasNextPage,
            result.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metadata));

        return Ok(result.Items);
    }
}
