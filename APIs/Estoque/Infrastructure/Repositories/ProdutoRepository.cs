using APIs.Estoque.Domain.Entities;
using APIs.Estoque.Infrastructure.Data;
using APIs.Estoque.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIs.Estoque.Infrastructure.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly EstoqueDbContexto _contexto;

        public ProdutoRepository(EstoqueDbContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<Produto> AdicionarProduto(Produto produto)
        {
            _contexto.Produtos.Add(produto);

            await _contexto.SaveChangesAsync();

            return produto;
        }
        public async Task<(List<Produto> Produtos, int TotalItems)> ObterTodosProdutosAsync(int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Max(pageSize, 1);

            var query = _contexto.Produtos.AsNoTracking();

            int totalItems = await query.CountAsync();

            var produtos = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (produtos, totalItems);
        }

        public async Task<Produto?> ObterProdutoPorId(int id) => await _contexto.Produtos.FindAsync(id);

        public async Task AtualizarAsync(Produto produto)
        {
            _contexto.Produtos.Update(produto);
            
            await _contexto.SaveChangesAsync();
        }

        public async Task RemoverProduto(Produto produto)
        {
            _contexto.Produtos.Remove(produto);

            await _contexto.SaveChangesAsync();
        }
  }
}