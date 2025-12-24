using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuvPizza.Domain.Entities;
using NuvPizza.Infrastructure.Services; 

namespace NuvPizza.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly TokenService _tokenService;

        public AuthController(UserManager<Usuario> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            throw new Exception("Teste de LOG-------------------------");
            // 1. Busca o usuário pelo email
            var user = await _userManager.FindByEmailAsync(loginModel.Email);

            // 2. Verifica se existe e se a senha confere
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                // 3. Gera o token
                var token = _tokenService.GenerateToken(user);
                
                // Retorna o token
                return Ok(new { token = token });
            }

            return Unauthorized("Usuário ou senha inválidos");
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}