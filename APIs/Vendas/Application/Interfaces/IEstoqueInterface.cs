using APIs.Vendas.Application.DTOs;

namespace APIs.Vendas.Application.Interfaces
{
    public interface IEstoqueService
    {
        Task<ProdutoResponseDTO> ObterProdutoPeloId(int id);
        Task VerificarEstoque(int produtoId, int quantidade);
    }
}