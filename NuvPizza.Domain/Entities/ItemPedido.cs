using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuvPizza.Domain.Entities
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public Guid PedidoId { get; set; }
        public string Nome { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal Total => Quantidade * PrecoUnitario;
    }
}