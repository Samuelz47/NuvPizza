using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Usuario usuario)
    {
        // 1. Lê a chave secreta que definimos nos User Secrets (ou appsettings)
        var keyString = _configuration["Jwt:Key"];
            
        // Segurança extra: Caso a chave não exista, lançamos erro para não gerar tokens inseguros
        if (string.IsNullOrEmpty(keyString)) 
            throw new Exception("Chave JWT não configurada!");

        var key = Encoding.ASCII.GetBytes(keyString);

        // 2. Define o que vai escrito no "Crachá" (Claims)
        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                // Guardamos o ID e o Email dentro do token
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email)
            }),
            Expires = DateTime.UtcNow.AddHours(2), // O token vale por 2 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        // 3. Gera o token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenConfig);

        return tokenHandler.WriteToken(token);
    }
}