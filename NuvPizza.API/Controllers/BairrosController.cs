using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult> GetAll()
    {
        return Ok(await _bairroRepository.GetAllAsync());
    }
}