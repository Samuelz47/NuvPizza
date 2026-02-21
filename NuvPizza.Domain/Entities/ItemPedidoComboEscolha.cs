using System;

namespace NuvPizza.Domain.Entities
{
    public class ItemPedidoComboEscolha
    {
        public int Id { get; set; }
        public int ItemPedidoId { get; set; }
        public ItemPedido ItemPedido { get; set; }

        public int ComboItemTemplateId { get; set; }
        public ComboItemTemplate ComboItemTemplate { get; set; }

        public int ProdutoEscolhidoId { get; set; }
        public Produto ProdutoEscolhido { get; set; }

        // Opcionais se for pizza meio a meio ou pizza com borda (dentro do combo)
        public int? ProdutoSecundarioId { get; set; }
        public Produto? ProdutoSecundario { get; set; }

        public int? BordaId { get; set; }
        public Produto? Borda { get; set; }
    }
}
