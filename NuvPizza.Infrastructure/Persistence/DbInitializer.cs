using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure.Persistence;

public class DbInitializer
{
    public static async Task SeedUsers(UserManager<Usuario> userManager, IConfiguration configuration)
    {
        var adminEmail = configuration["AdminSettings:Email"] ?? "admin@localhost"; 
        var adminPassword = configuration["AdminSettings:Password"];

        if (string.IsNullOrEmpty(adminPassword)) return;

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var usuario = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(usuario, adminPassword);

            if (resultado.Succeeded)
            {
                Console.WriteLine($"Admin ({adminEmail}) criado com sucesso!");
            }
        }
    }
}