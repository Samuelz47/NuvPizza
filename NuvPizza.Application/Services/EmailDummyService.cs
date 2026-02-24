using NuvPizza.Application.Interfaces;
using NuvPizza.Domain.Interfaces;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Services;

public class EmailDummyService : IEmailService
{
    public async Task EnviarEmailConfirmacao(Pedido pedido)
    {
        throw new NotImplementedException();
    }
}