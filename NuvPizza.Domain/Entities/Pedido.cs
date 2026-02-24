using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Domain.Entities
{
    public class Pedido
    {
        public Pedido() 
        {
            StatusPedido = StatusPedido.Criado;
            Id = Guid.NewGuid();
            DataPedido = DateTime.Now;
            Itens = new List<ItemPedido>();
        }
                
        public Guid Id { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public string TelefoneCliente { get; set; } = string.Empty;
        public string? EmailCliente { get; set; }
        public string Cep { get; set; } = string.Empty;
        public int Numero { get; set; }
        public string Complemento { get; set; } = string.Empty;
        public string BairroNome { get; set; } = string.Empty;
        public int BairroId { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public StatusPedido StatusPedido { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public List<ItemPedido> Itens { get; set; }
        public DateTime DataPedido { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorTotal { get; set; }
        public string? Observacao { get; set; }
    }
}