using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using APIs.Vendas.Domain.Entities;

namespace APIs.Vendas.Infrastructure.Configurations
{
    public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
    {
        public void Configure(EntityTypeBuilder<ItemPedido> builder)
        {
            builder.ToTable("ItensPedido");

            builder.HasKey(item => item.Id);

            builder.Property(item => item.ProdutoId)
                .IsRequired();

            builder.Property(item => item.Quantidade)
                .IsRequired();

            builder.Property(item => item.PrecoUnitario)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(item => item.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.HasOne(i => i.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(item => item.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}