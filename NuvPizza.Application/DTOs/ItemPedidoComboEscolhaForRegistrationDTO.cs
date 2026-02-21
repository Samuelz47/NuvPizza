namespace NuvPizza.Application.DTOs
{
    public class ItemPedidoComboEscolhaForRegistrationDTO
    {
        public int ComboItemTemplateId { get; set; }
        public int ProdutoEscolhidoId { get; set; }
        
        // Opcionais (Meio a meio / Borda)
        public int? ProdutoSecundarioId { get; set; }
        public int? BordaId { get; set; }
    }
}
