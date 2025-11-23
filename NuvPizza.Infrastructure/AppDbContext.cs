using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ItemPedido> Items { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Produto> Produtos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Pedido>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.NomeCliente).IsRequired().HasMaxLength(100);
                e.Property(p => p.EnderecoEntrega).IsRequired().HasMaxLength(200);
                e.Property(p => p.ValorTotal).HasColumnType("decimal(10,2)");
                
                // Relacionamento: Um Pedido tem muitos Itens
                e.HasMany(p => p.Itens)
                 .WithOne()
                 .HasForeignKey(i => i.Id);
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

            builder.Entity<Produto>(e => 
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Nome).IsRequired().HasMaxLength(100);
                e.Property(p => p.Descricao).HasMaxLength(250);
                e.Property(p => p.Preco).HasColumnType("decimal(10,2)"); // 10 digitos, 2 decimais
                e.Property(p => p.Tamanho).HasMaxLength(50);
            }); 
        } 
    }
}