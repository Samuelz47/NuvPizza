using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuvPizza.Domain.Enums;

namespace NuvPizza.Domain.Entities
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string ImagemUrl { get; set; } = string.Empty;
        public Categoria Categoria { get; set; }
        public Tamanho Tamanho { get; set; } = Tamanho.Unico;
        public bool Ativo { get; set; } = true;

        public ICollection<ComboItemTemplate> ComboTemplates { get; set; }
    }
}