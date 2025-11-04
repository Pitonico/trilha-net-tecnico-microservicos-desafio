using APIs.Estoque.Domain.Entities;
using APIs.Estoque.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace APIs.Estoque.Infrastructure.Data
{
    public class EstoqueDbContexto : DbContext
    {
        public EstoqueDbContexto(DbContextOptions<EstoqueDbContexto> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutoConfiguration).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Produto> Produtos { get; set; }        
    }
}