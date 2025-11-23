using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuvPizza.Domain.Entities
{
    public class Pedido
    {
        public Pedido() 
        {
            Id = Guid.NewGuid();
            DataPedido = DateTime.UtcNow;
            Itens = new List<ItemPedido>();
        }
                
        public Guid Id { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public string TelefoneCliente { get; set; } = string.Empty;
        public string EnderecoEntrega { get; set; } = string.Empty;
        public string FormaPagamento { get; set; } = string.Empty;
        public List<ItemPedido> Itens { get; set; }
        public DateTime DataPedido { get; set; }
        public decimal ValorTotal { get; set; }
    }
}