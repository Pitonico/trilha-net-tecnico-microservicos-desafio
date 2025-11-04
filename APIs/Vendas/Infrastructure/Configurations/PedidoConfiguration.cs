using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using APIs.Vendas.Domain.Entities;

namespace APIs.Vendas.Infrastructure.Configurations
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedidos");

            builder.HasKey(produto => produto.Id);

            builder.Property(produto => produto.DataCriacao)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(produto => produto.Total)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(produto => produto.Status)
                .IsRequired();

            builder.HasMany(produto => produto.Itens)
                .WithOne(item => item.Pedido)
                .HasForeignKey(item => item.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}