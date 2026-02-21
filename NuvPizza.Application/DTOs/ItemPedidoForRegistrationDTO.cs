namespace NuvPizza.Application.DTOs;

public class ItemPedidoForRegistrationDTO
{
    public int ProdutoId { get; set; }
    public int? ProdutoSecundarioId { get; set; }
    public int? BordaId { get; set; }
    public int Quantidade { get; set; }
    
    // Escolhas se esse item for um combo
    public List<ItemPedidoComboEscolhaForRegistrationDTO> EscolhasCombo { get; set; } = new List<ItemPedidoComboEscolhaForRegistrationDTO>();
}