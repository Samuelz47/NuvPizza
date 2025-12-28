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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Produto>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                e.Property(p => p.Descricao).HasMaxLength(250);
                e.Property(p => p.Preco).HasColumnType("decimal(10,2)"); 
                e.Property(p => p.Tamanho).HasMaxLength(50);
            });

            builder.Entity<ItemPedido>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Nome).IsRequired().HasMaxLength(100);
                e.Property(i => i.PrecoUnitario).HasColumnType("decimal(10,2)");
            });
            builder.Entity<ItemPedido>()
                .HasOne<Pedido>()
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoId);

            builder.Entity<Pedido>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.NomeCliente).IsRequired().HasMaxLength(100);
                e.Property(p => p.Cep).IsRequired().HasMaxLength(10);
                e.Property(p => p.Logradouro).IsRequired().HasMaxLength(150);
                e.Property(p => p.Numero).IsRequired().HasMaxLength(20);
                e.Property(p => p.ValorTotal).HasColumnType("decimal(10,2)");
                e.Property(p => p.ValorFrete).HasColumnType("decimal(10,2)");
                e.Property(p => p.Complemento).IsRequired(false).HasMaxLength(100);
        
                e.HasOne<Bairro>() 
                    .WithMany()
                    .HasForeignKey(p => p.BairroId)
                    .OnDelete(DeleteBehavior.Restrict);
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
        } 
    }
}