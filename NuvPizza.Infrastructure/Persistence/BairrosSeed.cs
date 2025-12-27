using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure.Persistence;

public class BairrosSeed
{
    public static List<Bairro> GetBairros()
    {
        return new List<Bairro>{
            new Bairro { Id = 1, Nome = "Guarapes", ValorFrete = 10.00m, Ativo = true },
            new Bairro { Id = 2, Nome = "Lagoa Nova", ValorFrete = 12.00m, Ativo = true },
            new Bairro { Id = 3, Nome = "Lagoa Seca", ValorFrete = 12.00m, Ativo = true },
            new Bairro { Id = 4, Nome = "Nossa Senhora de Nazaré", ValorFrete = 6.00m, Ativo = true },
            new Bairro { Id = 5, Nome = "Neópolis", ValorFrete = 15.00m, Ativo = true },
            new Bairro { Id = 6, Nome = "Nordeste", ValorFrete = 7.00m, Ativo = true },
            new Bairro { Id = 7, Nome = "Nova Descoberta", ValorFrete = 15.00m, Ativo = true },
            new Bairro { Id = 8, Nome = "Nova Parnamirim", ValorFrete = 20.00m, Ativo = true },
            new Bairro { Id = 9, Nome = "Pitimbu", ValorFrete = 15.00m, Ativo = true },

            new Bairro { Id = 10, Nome = "Alecrim", ValorFrete = 10.00m, Ativo = true },
            new Bairro { Id = 11, Nome = "Barro Vermelho", ValorFrete = 15.00m, Ativo = true },
            new Bairro { Id = 12, Nome = "Bom Pastor", ValorFrete = 4.00m, Ativo = true },
            new Bairro { Id = 13, Nome = "Candelária", ValorFrete = 10.00m, Ativo = true },
            new Bairro { Id = 14, Nome = "Capim Macio", ValorFrete = 18.00m, Ativo = true },
            new Bairro { Id = 15, Nome = "Cidade Nova", ValorFrete = 4.00m, Ativo = true },
            new Bairro { Id = 16, Nome = "Dix-Sept Rosado", ValorFrete = 8.00m, Ativo = true },
            new Bairro { Id = 17, Nome = "Cidade da Esperança", ValorFrete = 6.00m, Ativo = true },
            new Bairro { Id = 18, Nome = "Felipe Camarão", ValorFrete = 0.00m, Ativo = true }
        };
    }
}