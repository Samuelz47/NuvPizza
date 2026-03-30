using System;

namespace NuvPizza.Domain.Entities
{
    public class Motoboy
    {
        public Motoboy()
        {
            Id = Guid.NewGuid();
            Ativo = true;
            DataCadastro = DateTime.UtcNow.AddHours(-3);
        }

        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
