using Shared;
using Shared.DTOs;
using Shared.Enums;
using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Application.Interfaces;
using APIs.Vendas.Application.Mappers;
using APIs.Vendas.Application.Messaging.Events;
using APIs.Vendas.Domain.Entities;
using APIs.Vendas.Infrastructure.Interfaces;
using APIs.Vendas.Application.Messaging;

namespace APIs.Vendas.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _repositorio;
        private readonly IEstoqueService _estoqueService;
        private readonly RabbitMqPublisher _publisher;

        private readonly ILogger<PedidoService> _logger;

        public PedidoService(
            IPedidoRepository repositorio,
            IEstoqueService estoqueService,
            RabbitMqPublisher publisher,
            ILogger<PedidoService> logger)
        {
            _repositorio = repositorio;
            _estoqueService = estoqueService;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<PedidoResponseDTO> CriarPedido(PedidoRequestDTO pedidoRequestDTO)
        {
            if (!Enum.IsDefined(pedidoRequestDTO.Status))
            {
                _logger.LogWarning($"Status {pedidoRequestDTO.Status} inválido.");
                throw new ArgumentOutOfRangeException(nameof(pedidoRequestDTO.Status), "Status inválido.");
            }
                
            List<ItemPedido> itens = [];

            foreach (ItemPedidoRequestDTO item in pedidoRequestDTO.Itens)
            {
                _logger.LogDebug("Verificando estoque para produto {ProdutoId} com quantidade {Qtd}.",
                    item.ProdutoId, item.Quantidade);

                await _estoqueService.VerificarEstoque(item.ProdutoId, item.Quantidade);

                ProdutoResponseDTO produto = await _estoqueService.ObterProdutoPeloId(item.ProdutoId);

                itens.Add(new ItemPedido
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco,
                    SubTotal = item.Quantidade * produto.Preco
                });
            }

            Pedido pedido = new Pedido
            {
                Itens = itens,
                Status = pedidoRequestDTO.Status
            };

            pedido.CalcularValorTotal();

            Pedido pedidoSalvo = await _repositorio.CriarPedido(pedido);

            _logger.LogInformation("Pedido {PedidoId} persistindo no banco com sucesso no valor total de {Total}.",
                pedidoSalvo.Id, pedidoSalvo.Total);

            await _publisher.PublicarAsync("pedido.criado", new PedidoCriadoEvent(
                pedidoSalvo.Id,
                pedidoSalvo.Itens.Select(i => new ItemPedidoEvent(i.ProdutoId, i.Quantidade)).ToList()
            ));

            _logger.LogInformation("Evento 'pedido.criado' publicado para o Pedido {PedidoId}.", pedidoSalvo.Id);

            return PedidoMapper.ParaResponseDTO(pedidoSalvo);
        }

        public async Task<PedidoResponseDTO> ObterPedidoPorId(int id)
        {
            Pedido? pedido = await _repositorio.ObterPedidoProId(id);

            if(pedido == null)
            {
                _logger.LogWarning("Tentativa de obter pedido inexistente ID {Id}.", id);
                throw new KeyNotFoundException("Pedido não encontrado.");  
            }
        
            _logger.LogInformation("Pedido obtido do banco com sucesso: {Pedido}", pedido);

            return PedidoMapper.ParaResponseDTO(pedido);
        }

        public async Task<PagedResult<PedidoResponseDTO>> ObterTodosPedidos(int pageNumber = 1, int pageSize = 10)
        {

            var (pedidos, totalItems) = await _repositorio.ObterTodosPedidos(pageNumber, pageSize);

            _logger.LogInformation("Retornando {QtdPedidos} pedidos de um total de {Total}.", pedidos.Count, totalItems);

            var items = pedidos.Select(PedidoMapper.ParaResponseDTO).ToList();

            return new PagedResult<PedidoResponseDTO>
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task AtualizarStatus(int id, StatusPedidoEnum novoStatus)
        {
            if (!Enum.IsDefined(novoStatus))
            {
                _logger.LogWarning("Tentativa de atualizar pedido {PedidoId} com status inválido: {Status}.", id, novoStatus);
                throw new ArgumentOutOfRangeException(nameof(novoStatus), "Status inválido.");
            }
                
            Pedido? pedido = await _repositorio.ObterPedidoProId(id);

            if(pedido == null)
            {
                _logger.LogWarning("Tentativa de obter pedido inexistente ID {Id}.", id);
                throw new KeyNotFoundException("Pedido não encontrado.");  
            }

            if (pedido.Status == StatusPedidoEnum.Concluido)
            {
                _logger.LogWarning("Tentativa de alterar pedido {PedidoId}, mas ele já está concluído.", id);
                throw new InvalidOperationException("Não é possível alterar um pedido já concluído.");
            }
               
            await _repositorio.AtualizarStatus(pedido, novoStatus);

            _logger.LogInformation("Status do pedido {PedidoId} atualizado com sucesso para {novoStatus}.",
                pedido.Id, novoStatus);
        }
    }
}