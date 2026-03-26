using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<Usuario>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ItemPedido> Items { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Bairro> Bairros { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<ComboItemTemplate> ComboTemplates { get; set; }
        public DbSet<ItemPedidoComboEscolha> EscolhasCombo { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Produto>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                e.Property(p => p.Descricao).HasMaxLength(250);
                e.Property(p => p.Preco).HasColumnType("decimal(10,2)"); 
                e.Property(p => p.PrecoPromocional).HasColumnType("decimal(10,2)"); 
                e.Property(p => p.Tamanho).HasMaxLength(50);
            });

            builder.Entity<ComboItemTemplate>(e => 
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.ValorCobertura).HasColumnType("decimal(10,2)");
                e.HasOne(c => c.Produto)
                 .WithMany(p => p.ComboTemplates)
                 .HasForeignKey(c => c.ProdutoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ItemPedido>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Nome).IsRequired().HasMaxLength(100);
                e.Property(i => i.PrecoUnitario).HasColumnType("decimal(10,2)");
            });

            builder.Entity<ItemPedidoComboEscolha>(e => 
            {
                e.HasKey(c => c.Id);
                e.HasOne(c => c.ItemPedido)
                 .WithMany(i => i.EscolhasCombo)
                 .HasForeignKey(c => c.ItemPedidoId)
                 .OnDelete(DeleteBehavior.Cascade);
                 
                e.HasOne(c => c.ComboItemTemplate)
                 .WithMany()
                 .HasForeignKey(c => c.ComboItemTemplateId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(c => c.ProdutoEscolhido)
                 .WithMany()
                 .HasForeignKey(c => c.ProdutoEscolhidoId)
                 .OnDelete(DeleteBehavior.Restrict);
                 
                e.HasOne(c => c.ProdutoSecundario)
                 .WithMany()
                 .HasForeignKey(c => c.ProdutoSecundarioId)
                 .OnDelete(DeleteBehavior.Restrict);
                 
                e.HasOne(c => c.Borda)
                 .WithMany()
                 .HasForeignKey(c => c.BordaId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ItemPedido>()
                .HasOne<Pedido>()
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoId);

            builder.Entity<Pedido>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.NomeCliente).IsRequired().HasMaxLength(100);
                e.Property(p => p.Cep).IsRequired(false).HasMaxLength(15);
                e.Property(p => p.Logradouro).IsRequired(false).HasMaxLength(180);
                e.Property(p => p.PontoReferencia).IsRequired(false).HasMaxLength(150);
                e.Property(p => p.IsRetirada).IsRequired().HasDefaultValue(false);
                e.Property(p => p.Numero).HasMaxLength(30);
                e.Property(p => p.ValorTotal).HasColumnType("decimal(10,2)");
                e.Property(p => p.ValorFrete).HasColumnType("decimal(10,2)");
                e.Property(p => p.Complemento).IsRequired(false).HasMaxLength(100);

                e.Property(p => p.FormaPagamento).HasConversion<string>();
                
                e.HasOne<Bairro>() 
                    .WithMany()
                    .HasForeignKey(p => p.BairroId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Cliente)
                    .WithMany(c => c.Pedidos)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Cliente>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Nome).IsRequired().HasMaxLength(100);
                e.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
                e.HasIndex(c => c.Telefone).IsUnique();
                e.Property(c => c.Email).HasMaxLength(150);
                e.Property(c => c.ValorTotalGasto).HasColumnType("decimal(10,2)");
            });

            builder.Entity<Bairro>().HasData(BairrosSeed.GetBairros());
            builder.Entity<Bairro>(e =>
            {
                e.HasKey(b => b.Id);
                e.Property(b => b.Nome).IsRequired().HasMaxLength(100);
                e.Property(b => b.ValorFrete).HasColumnType("decimal(10,2)"); 
            });
            
            builder.Entity<Configuracao>().HasData(
                new Configuracao 
                { 
                    Id = 1, 
                    EstaAberta = false, // Nasce fechada (Segurança)
                    DataHoraFechamentoAtual = null,
                    HorarioFechamentoPadrao = new TimeSpan(23, 0, 0) // Sugestão: 23h
                }
            );

            builder.Entity<Cupom>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Codigo).IsRequired().HasMaxLength(30);
                e.HasCheckConstraint("CK_Cupom_Codigo_SemEspacos", "\"Codigo\" NOT LIKE '% %'");
                e.HasIndex(c => c.Codigo).IsUnique();
                e.Property(c => c.DescontoPorcentagem).HasColumnType("decimal(5,2)");
                e.Property(c => c.PedidoMinimo).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            });
        } 
    }
}