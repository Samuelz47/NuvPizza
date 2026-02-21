using NuvPizza.Domain.Enums;

namespace NuvPizza.Application.DTOs
{
    public class ComboItemTemplateDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public Categoria CategoriaPermitida { get; set; }
        public Tamanho TamanhoObrigatorio { get; set; }
    }
}
