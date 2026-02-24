using NuvPizza.Domain.Entities;

namespace NuvPizza.Domain.Interfaces;

public interface IEmailService
{
    Task EnviarEmailConfirmacao(Pedido pedido);
}