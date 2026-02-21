using System;
using System.Collections.Generic;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Domain.Entities
{
    public class ComboItemTemplate
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        
        [System.Text.Json.Serialization.JsonIgnore]
        public Produto Produto { get; set; }
        
        // Exemplo: 1 Pizza Grande, ou 2 Bebidas
        public int Quantidade { get; set; }
        
        // A regra que o cliente precisa seguir para preencher este slot
        public Categoria CategoriaPermitida { get; set; }
        public Tamanho TamanhoObrigatorio { get; set; }
    }
}
