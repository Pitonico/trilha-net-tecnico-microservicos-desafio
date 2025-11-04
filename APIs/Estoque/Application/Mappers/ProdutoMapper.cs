using APIs.Estoque.Application.DTOs;
using APIs.Estoque.Domain.Entities;
using Shared.DTOs;

namespace APIs.Estoque.Application.Mappers
{
    public static class ProdutoMapper
    {
        public static Produto ParaEntidade(ProdutoRequestDTO produtoRequestDTO)
        {
            return new Produto
            {
                Nome = produtoRequestDTO.Nome,
                Descricao = produtoRequestDTO.Descricao,
                Preco = produtoRequestDTO.Preco,
                QuantidadeEstoque = produtoRequestDTO.QuantidadeEstoque
            };
        }

        public static ProdutoResponseDTO ParaResponseDTO(Produto produto)
        {
            return new ProdutoResponseDTO
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco,
                QuantidadeEstoque = produto.QuantidadeEstoque
            };
        }

        public static void AtualizarEntidade(Produto produto, ProdutoRequestDTO produtoRequestDTO)
        {
            produto.Nome = produtoRequestDTO.Nome;
            produto.Descricao = produtoRequestDTO.Descricao;
            produto.Preco = produtoRequestDTO.Preco;
            produto.QuantidadeEstoque = produtoRequestDTO.QuantidadeEstoque;
        }
    }
}