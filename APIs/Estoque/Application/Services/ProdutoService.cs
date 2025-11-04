using APIs.Estoque.Application.DTOs;
using APIs.Estoque.Application.Interfaces;
using APIs.Estoque.Application.Mappers;
using APIs.Estoque.Application.Messaging;
using APIs.Estoque.Application.Messaging.Events;
using APIs.Estoque.Domain.Entities;
using APIs.Estoque.Infrastructure.Interfaces;
using Shared;
using Shared.DTOs;

namespace APIs.Estoque.Application.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _repositorio;

        private readonly RabbitMqPublisher _publisher;
        private readonly ILogger _logger;
        
        public ProdutoService(IProdutoRepository repositorio, ILogger<ProdutoService> logger, RabbitMqPublisher publisher)
        {
            _repositorio = repositorio;
            _logger = logger;
            _publisher = publisher;
        }
        public async Task<ProdutoResponseDTO> AdicionarProduto(ProdutoRequestDTO produtoRequestDTO)
        {
            Produto produto = ProdutoMapper.ParaEntidade(produtoRequestDTO);

            Produto produtoSalvo = await _repositorio.AdicionarProduto(produto);

            _logger.LogInformation("Produto persistido no banco com sucesso. ID: {ProdutoId}", produtoSalvo.Id);

            return ProdutoMapper.ParaResponseDTO(produtoSalvo);
        }

        public async Task<PagedResult<ProdutoResponseDTO>> ObterTodosProdutos(int pageNumber = 1, int pageSize = 10)
        {
            var (produtos, totalItems) = await _repositorio.ObterTodosProdutosAsync(pageNumber, pageSize);

            List<ProdutoResponseDTO> produtosDTO = produtos
                .Select(ProdutoMapper.ParaResponseDTO)
                .ToList();

            _logger.LogInformation("Total de produtos obtidos do banco com sucesso: {Total}", totalItems);

            return new PagedResult<ProdutoResponseDTO>
            {
                Items = produtosDTO,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<ProdutoResponseDTO> ObterProdutoPorId(int id)
        {
            Produto? produtoObtido = await _repositorio.ObterProdutoPorId(id);

            if(produtoObtido == null)
            {
                _logger.LogWarning("Tentativa de obter produto inexistente ID {Id}", id);
                throw new KeyNotFoundException("Produto não encontrado");  
            }

            _logger.LogInformation("Produto obtido do banco com sucesso: {Produto}", produtoObtido);

            return ProdutoMapper.ParaResponseDTO(produtoObtido);
        }
        
        public async Task AtualizarProduto(int id, ProdutoRequestDTO produtoRequestDTO)
        {
            Produto? produto = await _repositorio.ObterProdutoPorId(id);

            if(produto == null)
            {
                _logger.LogWarning("Tentativa de atualizar produto inexistente ID {Id}", id);
                throw new KeyNotFoundException("Produto não encontrado");  
            }

            ProdutoMapper.AtualizarEntidade(produto, produtoRequestDTO);

            await _repositorio.AtualizarAsync(produto);

            _logger.LogInformation("Produto persistido no banco com sucesso. ID: {Produto}", produto);
        }
        public async Task RemoverProduto(int id)
        {
            Produto? produto = await _repositorio.ObterProdutoPorId(id);

            if(produto == null)
            {
                _logger.LogWarning("Tentativa de remover produto inexistente ID {Id}", id);
                throw new KeyNotFoundException("Produto não encontrado");  
            }

            await _repositorio.RemoverProduto(produto);

            _logger.LogInformation("Produto removindo no banco com sucesso. ID: {ProdutoId}", produto.Id);
        }

        public async Task ReduzirEstoque(int id, int quantidade)
        {
            _logger.LogInformation("Reduzindo estoque do produto ID {Id} em {Quantidade}", id, quantidade);

            Produto? produto = await _repositorio.ObterProdutoPorId(id);
                
            if(produto == null)
            {
                _logger.LogWarning("Tentativa de reduzir estoque de produto inexistente ID {Id}", id);
                throw new KeyNotFoundException("Produto não encontrado");  
            }

            if (produto.QuantidadeEstoque < quantidade)
            {
                _logger.LogWarning("Estoque insuficiente para o produto ID {Id}. Estoque atual: {EstoqueAtual}, Tentativa: {Quantidade}",
                    id, produto.QuantidadeEstoque, quantidade);
                throw new InvalidOperationException("Estoque insuficiente para a venda.");
            }
                
            produto.QuantidadeEstoque -= quantidade;

            await _repositorio.AtualizarAsync(produto);

            EstoqueAtualizadoEvent evento = new(id, quantidade);

            await _publisher.PublicarAsync("estoque.atualizado", evento);

            _logger.LogInformation("[RabbitMQ] Evento EstoqueAtualizado publicado para Produto {ProdutoId}", id);

        }
    }
}