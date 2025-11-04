using APIs.Estoque.Application.DTOs;
using Shared;
using Shared.DTOs;

namespace APIs.Estoque.Application.Interfaces
{
    public interface IProdutoService
    {
        Task<ProdutoResponseDTO> AdicionarProduto(ProdutoRequestDTO produtoDto);
        Task<ProdutoResponseDTO> ObterProdutoPorId(int id);
        Task<PagedResult<ProdutoResponseDTO>> ObterTodosProdutos(int pageNumber, int pageSize);
        Task AtualizarProduto(int id, ProdutoRequestDTO produtoDto);

        Task ReduzirEstoque(int id, int quantidade);
        Task RemoverProduto(int id);
    }
}