using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]

public class BairrosController : ControllerBase
{
    private readonly IBairroRepository _bairroRepository;

    public BairrosController(IBairroRepository bairroRepository)
    {
        _bairroRepository = bairroRepository;
    }

    [HttpGet]
    [EnableRateLimiting("PublicApiLimit")]
    public async Task<ActionResult> GetAll()
    {
        return Ok(await _bairroRepository.GetAllAsync());
    }
}