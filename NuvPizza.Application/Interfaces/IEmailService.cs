using NuvPizza.Domain.Entities;

namespace NuvPizza.Application.Interfaces;

public interface IEmailService
{
    Task EnviarEmailConfirmacao(Pedido pedido);
}