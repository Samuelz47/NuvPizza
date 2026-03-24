namespace NuvPizza.Domain.Pagination;

public class ClienteRankingParameters : QueryParameters
{
    // "valor" or "pedidos" — default sorts by total spent
    public string OrdenarPor { get; set; } = "valor";
}
