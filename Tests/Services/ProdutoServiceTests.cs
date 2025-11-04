using APIs.Estoque.Application.Services;
using APIs.Estoque.Domain.Entities;
using APIs.Estoque.Infrastructure.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs;

namespace APIs.Estoque.Tests.Application.Services
{
    public class ProdutoServiceTests
    {
        private readonly Mock<IProdutoRepository> _mockRepo;
        private readonly Mock<ILogger<ProdutoService>> _mockLogger;
        private readonly ProdutoService _service;

        public ProdutoServiceTests()
        {
            _mockRepo = new Mock<IProdutoRepository>();
            _mockLogger = new Mock<ILogger<ProdutoService>>();
            _service = new ProdutoService(_mockRepo.Object,  _mockLogger.Object);
        }

        [Fact]
        public async Task AdicionarProduto_DeveRetornarProdutoResponseDTO()
        {
            // Arrange
            ProdutoRequestDTO dto = new()
            { Nome = "Teclado", Descricao = "Mecânico", Preco = 250, QuantidadeEstoque = 5 };
                
            Produto produtoSalvo = new() 
            { Id = 1, Nome = "Teclado", Descricao = "Mecânico", Preco = 250, QuantidadeEstoque = 5 };

            // Configuração informando o que o mock deve fazer quando o AdicionarProduto do repositorio for chamado
            _mockRepo.Setup(r => r.AdicionarProduto(It.IsAny<Produto>()))
                .ReturnsAsync(produtoSalvo);

            // Act
            var result = await _service.AdicionarProduto(dto);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be("Teclado");
            result.Descricao.Should().Be("Mecânico");
            result.Preco.Should().Be(250);
            result.QuantidadeEstoque.Should().Be(5);

            _mockRepo.Verify(r => r.AdicionarProduto(It.IsAny<Produto>()), Times.Once);
        }

        [Fact]
        public async Task ObterProdutoPorId_DeveRetornarProduto_QuandoExistir()
        {
            // Arrange
            var produto = new Produto 
            { Id = 1, Nome = "Mouse", Descricao = "Sem fio", Preco = 100, QuantidadeEstoque = 10 };
                
            _mockRepo.Setup(r => r.ObterProdutoPorId(1)).ReturnsAsync(produto);

            // Act
            var result = await _service.ObterProdutoPorId(1);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(produto.Nome);
            result.Descricao.Should().Be(produto.Descricao);
            result.Preco.Should().Be(produto.Preco);
            result.QuantidadeEstoque.Should().Be(produto.QuantidadeEstoque);
        }

        [Fact]
        public async Task ObterProdutoPorId_DeveLancarExcecao_QuandoNaoExistir()
        {
            _mockRepo.Setup(r => r.ObterProdutoPorId(1))
                .ReturnsAsync((Produto)null);

            var action = async () => await _service.ObterProdutoPorId(1);

            await action
                .Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Produto não encontrado");
        }

        [Fact]
        public async Task ObterTodosProdutos_DeveRetornarListaPaginada()
        {
            var produtos = new List<Produto>
            {
                new() { Id = 1, Nome = "Produto 1" },
                new() { Id = 2, Nome = "Produto 2" }
            };

            _mockRepo.Setup(r => r.ObterTodosProdutosAsync(1, 10))
                     .ReturnsAsync((produtos, produtos.Count));

            // Act
            var result = await _service.ObterTodosProdutos();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalItems.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task AtualizarProduto_DeveAtualizarEntidade()
        {
            var produtoExistente = new Produto { Id = 1, Nome = "Antigo", QuantidadeEstoque = 5 };
            var dto = new ProdutoRequestDTO { Nome = "Novo", QuantidadeEstoque = 10 };

            _mockRepo.Setup(r => r.ObterProdutoPorId(1)).ReturnsAsync(produtoExistente);
            _mockRepo.Setup(r => r.AtualizarAsync(produtoExistente)).Returns(Task.CompletedTask);

            await _service.AtualizarProduto(1, dto);

            _mockRepo.Verify(r => r.AtualizarAsync(It.Is<Produto>(p => p.Nome == "Novo")), Times.Once);
        }

        [Fact]
        public async Task AtualizarProduto_DeveLancarExcecao_QuandoNaoExistir()
        {
            _mockRepo.Setup(r => r.ObterProdutoPorId(1))
                     .ReturnsAsync((Produto)null);

            var action = async () => await _service.AtualizarProduto(1, new ProdutoRequestDTO() { Nome = "" });

            await action.Should().ThrowAsync<KeyNotFoundException>()
                        .WithMessage("Produto não encontrado");
        }

        [Fact]
        public async Task RemoverProduto_DeveRemoverComSucesso()
        {
            var produto = new Produto { Id = 1, Nome = "Produto" };
            _mockRepo.Setup(r => r.ObterProdutoPorId(1)).ReturnsAsync(produto);

            await _service.RemoverProduto(1);

            _mockRepo.Verify(r => r.RemoverProduto(produto), Times.Once);
        }

        [Fact]
        public async Task ReduzirEstoque_DeveDiminuirQuantidade_QuandoValido()
        {
            var produto = new Produto { Id = 1, Nome = "Produto", QuantidadeEstoque = 10 };
            _mockRepo.Setup(r => r.ObterProdutoPorId(1)).ReturnsAsync(produto);

            await _service.ReduzirEstoque(1, 5);

            produto.QuantidadeEstoque.Should().Be(5);
            _mockRepo.Verify(r => r.AtualizarAsync(It.IsAny<Produto>()), Times.Once);
        }

        [Fact]
        public async Task ReduzirEstoque_DeveLancarExcecao_QuandoEstoqueInsuficiente()
        {
            var produto = new Produto { Id = 1, Nome = "Produto", QuantidadeEstoque = 2 };
            _mockRepo.Setup(r => r.ObterProdutoPorId(1)).ReturnsAsync(produto);

            var action = async () => await _service.ReduzirEstoque(1, 5);

            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("Estoque insuficiente para a venda.");
        }

        [Fact]
        public async Task ReduzirEstoque_DeveLancarExcecao_QuandoProdutoNaoExistir()
        {
            _mockRepo.Setup(r => r.ObterProdutoPorId(1))
                     .ReturnsAsync((Produto)null);

            var action = async () => await _service.ReduzirEstoque(1, 1);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                        .WithMessage("Produto não encontrado");
        }
    }
}
