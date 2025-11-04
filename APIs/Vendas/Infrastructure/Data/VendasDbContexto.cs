using Microsoft.EntityFrameworkCore;
using APIs.Vendas.Domain.Entities;
using APIs.Vendas.Infrastructure.Configurations;

namespace APIs.Vendas.Infrastructure.Data
{
    public class VendasDbContexto : DbContext
    {
        public VendasDbContexto(DbContextOptions<VendasDbContexto> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidoConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    }

}