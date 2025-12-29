namespace NuvPizza.Infrastructure.Services;

public interface IViaCepService
{
    Task<ViaCepResponse?> CheckAsync(string cep);
}