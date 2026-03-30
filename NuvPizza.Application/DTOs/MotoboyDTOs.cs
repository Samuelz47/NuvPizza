using System;

namespace NuvPizza.Application.DTOs
{
    public class MotoboyDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    public class MotoboyCreateDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
    }

    public class MotoboyUpdateDTO
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class FaturamentoMotoboyDTO
    {
        public Guid MotoboyId { get; set; }
        public string NomeMotoboy { get; set; } = string.Empty;
        public decimal TotalFrete { get; set; }
        public int QuantidadeEntregas { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
    }
}
