using FluentAssertions;
using Moq;
using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Application.Messaging.Events;
using APIs.Vendas.Application.Services;
using APIs.Vendas.Domain.Entities;
using APIs.Vendas.Infrastructure.Interfaces;
using APIs.Vendas.Application.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Microsoft.Extensions.Logging;
using Vendas.Application.Interfaces;

namespace APIs.Vendas.Tests.Application.Services
{
    public class PedidoServiceTests
    {
        private readonly Mock<IPedidoRepository> _mockRepo;
        private readonly Mock<IEstoqueService> _mockEstoque;
        private readonly Mock<IRabbitMqPublisher> _mockPublisher;
        private readonly Mock<ILogger<PedidoService>> _mockLogger;
        private readonly PedidoService _service;

        public PedidoServiceTests()
        {
            _mockRepo = new Mock<IPedidoRepository>();
            _mockEstoque = new Mock<IEstoqueService>();
            _mockPublisher = new Mock<IRabbitMqPublisher>();
            _mockLogger = new Mock<ILogger<PedidoService>>();
            _service = new PedidoService(_mockRepo.Object, _mockEstoque.Object, _mockPublisher.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CriarPedido_DeveCriarPedidoComSucesso()
        {
            // Arrange
            var dto = new PedidoRequestDTO
            {
                Status = StatusPedidoEnum.Pendente,
                Itens = new List<ItemPedidoRequestDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 2 }
                }
                
            };

            _mockEstoque.Setup(e => e.VerificarEstoque(1, 2)).Returns(Task.CompletedTask);
            _mockEstoque.Setup(e => e.ObterProdutoPeloId(1))
                .ReturnsAsync(new ProdutoResponseDTO { Id = 1, Nome = "Produto", Preco = 10 });

            var pedidoSalvo = new Pedido
            {
                Id = 99,
                Status = StatusPedidoEnum.Pendente,
                Itens = new List<ItemPedido>
                {
                    new() { ProdutoId = 1, Quantidade = 2, PrecoUnitario = 10, SubTotal = 20 }
                },
                Total = 20
            };

            _mockRepo.Setup(r => r.CriarPedido(It.IsAny<Pedido>()))
                     .ReturnsAsync(pedidoSalvo);

            _mockPublisher.Setup(p => p.PublicarAsync(It.IsAny<string>(), It.IsAny<PedidoCriadoEvent>()))
                          .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CriarPedido(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(99);
            result.Itens.Should().HaveCount(1);
            result.Total.Should().Be(20);

            _mockRepo.Verify(r => r.CriarPedido(It.IsAny<Pedido>()), Times.Once);
            _mockPublisher.Verify(p => p.PublicarAsync("pedido.criado", It.IsAny<PedidoCriadoEvent>()), Times.Once);
        }

        [Fact]
        public async Task CriarPedido_DeveLancarExcecao_QuandoStatusInvalido()
        {
            var pedidoRequest = new PedidoRequestDTO
            {
                Status = (StatusPedidoEnum)999,
                Itens = new List<ItemPedidoRequestDTO>()
            };

            var action = async () => await _service.CriarPedido(pedidoRequest);

            await action.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithMessage("Status inválido.*");
        }

        [Fact]
        public async Task ObterPedidoPorId_DeveRetornarPedido()
        {
            var pedido = new Pedido { Id = 1, Status = StatusPedidoEnum.Pendente, Itens = new List<ItemPedido>() };

            _mockRepo.Setup(r => r.ObterPedidoProId(1))
                     .ReturnsAsync(pedido);

            var result = await _service.ObterPedidoPorId(1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task ObterPedidoPorId_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            _mockRepo.Setup(r => r.ObterPedidoProId(1))
                     .ReturnsAsync((Pedido)null);

            var action = async () => await _service.ObterPedidoPorId(1);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                        .WithMessage("Pedido não encontrado.");
        }

        [Fact]
        public async Task ObterTodosPedidos_DeveRetornarListaPaginada()
        {
            var pedidos = new List<Pedido>
            {
                new() { Id = 1, Status = StatusPedidoEnum.Pendente },
                new() { Id = 2, Status = StatusPedidoEnum.Concluido }
            };

            _mockRepo.Setup(r => r.ObterTodosPedidos(1, 10))
                     .ReturnsAsync((pedidos, pedidos.Count));

            var result = await _service.ObterTodosPedidos();

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalItems.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task AtualizarStatus_DeveAtualizarComSucesso()
        {
            var pedido = new Pedido { Id = 1, Status = StatusPedidoEnum.Pendente };

            _mockRepo.Setup(r => r.ObterPedidoProId(1))
                     .ReturnsAsync(pedido);

            _mockRepo.Setup(r => r.AtualizarStatus(pedido, StatusPedidoEnum.Concluido))
                     .Returns(Task.CompletedTask);

            await _service.AtualizarStatus(1, StatusPedidoEnum.Concluido);

            _mockRepo.Verify(r => r.AtualizarStatus(pedido, StatusPedidoEnum.Concluido), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatus_DeveLancarExcecao_QuandoStatusInvalido()
        {
            var action = async () => await _service.AtualizarStatus(1, (StatusPedidoEnum)999);

            await action.Should().ThrowAsync<ArgumentOutOfRangeException>()
                        .WithMessage("Status inválido.*");
        }

        [Fact]
        public async Task AtualizarStatus_DeveLancarExcecao_QuandoPedidoNaoEncontrado()
        {
            _mockRepo.Setup(r => r.ObterPedidoProId(1))
                     .ReturnsAsync((Pedido)null);

            var action = async () => await _service.AtualizarStatus(1, StatusPedidoEnum.Pendente);

            await action.Should().ThrowAsync<KeyNotFoundException>()
                        .WithMessage("Pedido não encontrado.");
        }

        [Fact]
        public async Task AtualizarStatus_DeveLancarExcecao_QuandoPedidoJaConcluido()
        {
            var pedido = new Pedido { Id = 1, Status = StatusPedidoEnum.Concluido };

            _mockRepo.Setup(r => r.ObterPedidoProId(1))
                     .ReturnsAsync(pedido);

            var action = async () => await _service.AtualizarStatus(1, StatusPedidoEnum.Pendente);

            await action.Should().ThrowAsync<InvalidOperationException>()
                        .WithMessage("Não é possível alterar um pedido já concluído.");
        }
    }
}
