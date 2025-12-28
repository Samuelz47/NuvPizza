using Microsoft.AspNetCore.Mvc;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;

namespace NuvPizza.API.Controllers;

[ApiController]
[Route("[controller]")]

public class BairrosController : ControllerBase
{
    private readonly IRepository<Bairro> _bairroRepository;

    public BairrosController(IRepository<Bairro> bairroRepository)
    {
        _bairroRepository = bairroRepository;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        return Ok(await _bairroRepository.GetAllAsync());
    }
}