using APIs.Estoque.Domain.Entities;

namespace APIs.Estoque.Infrastructure.Interfaces
{
    public interface IProdutoRepository
    {
        public Task<Produto> AdicionarProduto(Produto produto);
        public Task<(List<Produto> Produtos, int TotalItems)> ObterTodosProdutosAsync(int pageNumber, int pageSize);
        public Task<Produto?> ObterProdutoPorId(int id);

        public Task AtualizarAsync(Produto produto);

        public Task RemoverProduto(Produto produto);
    }
}