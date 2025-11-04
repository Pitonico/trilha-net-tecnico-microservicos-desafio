using APIs.Estoque.Application.Messaging;
using APIs.Estoque.Application.Interfaces;
using APIs.Estoque.Application.Messaging.Events;

namespace APIs.Estoque.Application.Services
{
    public class PedidoConsumerService : BackgroundService
    {
        private readonly RabbitMqConsumer<PedidoCriadoEvent> _consumer;
        private readonly ILogger<PedidoConsumerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public PedidoConsumerService(RabbitMqConsumer<PedidoCriadoEvent> consumer, ILogger<PedidoConsumerService> logger, IServiceScopeFactory scopeFactory)
        {
            _consumer = consumer;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando consumo de pedidos...");
            await _consumer.StartConsumingAsync("pedido.criado", ProcessarPedidoAsync, stoppingToken);
        }

        private async Task ProcessarPedidoAsync(PedidoCriadoEvent evento)
        {
            using var scope = _scopeFactory.CreateScope();
            var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();

            _logger.LogInformation("Processando pedido {PedidoId}", evento.PedidoId);
            foreach (var item in evento.Itens)
            {
                _logger.LogInformation("Reduzindo estoque do Produto {ProdutoId} em {Quantidade}", item.ProdutoId, item.Quantidade);
                await produtoService.ReduzirEstoque(item.ProdutoId, item.Quantidade);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consumer.DisposeAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}