namespace NuvPizza.Application.Interfaces;

public interface INotificacaoService
{
    Task NotificarAlteracaoStatusLoja(bool estaAberta, string mensagem);
    Task NotificarFechamento(int minutosRestantes);
}