using APIs.Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIs.Estoque.Infrastructure.Configurations
{
  public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
  {
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
            builder.ToTable("Produtos");
            builder.HasKey(produto => produto.Id);
            builder.Property(produto => produto.Nome)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(produto => produto.Descricao)
                .HasMaxLength(250);
            builder.Property(produto => produto.Preco)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(produto => produto.QuantidadeEstoque)
                .IsRequired();
    }
  }
}