using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Vendas.Application.Interfaces;

namespace APIs.Vendas.Application.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
    {
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private bool _disposed;

        public RabbitMqPublisher(IConfiguration config, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;

            _factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"]!,
                UserName = config["RabbitMQ:UserName"]!,
                Password = config["RabbitMQ:Password"]!
            };
        }

        private async Task<IConnection> GetConnectionAsync()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _connection = await _factory.CreateConnectionAsync();
            _logger.LogInformation("[RabbitMQ] Conexão estabelecida com {Host}", _factory.HostName);

            return _connection;
        }

        public async Task PublicarAsync<T>(string queueName, T mensagemObj)
        {
            int retryCount = 0;
            const int maxRetries = 5;
            const int retryDelayMs = 3000;

            var mensagem = JsonSerializer.Serialize(mensagemObj);
            var body = Encoding.UTF8.GetBytes(mensagem);

            while (true)
            {
                try
                {
                    var connection = await GetConnectionAsync();
                    await using var channel = await connection.CreateChannelAsync();

                    await channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var props = new BasicProperties
                    {
                        DeliveryMode = DeliveryModes.Persistent
                    };

                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        basicProperties: props,
                        body: body
                    );

                    _logger.LogInformation(
                        "[RabbitMQ] ✅ Mensagem publicada. Fila: {Queue}, Tamanho: {Size} bytes, Tipo: {Type}",
                        queueName, body.Length, typeof(T).Name
                    );
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    _logger.LogWarning(
                        "[RabbitMQ] ⚠️ Falha ao publicar (tentativa {Retry}/{Max}, aguardando {Delay}ms). Erro: {Erro}",
                        retryCount, maxRetries, retryDelayMs, ex.Message
                    );

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(
                            "[RabbitMQ] ❌ Limite máximo de tentativas atingido. Abortando publicação. Erro final: {Erro}",
                            ex.Message
                        );
                        throw;
                    }

                    await Task.Delay(retryDelayMs);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            try
            {
                if (_connection != null)
                {
                    await _connection.DisposeAsync();
                    _logger.LogInformation("[RabbitMQ] Conexão encerrada com sucesso.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[RabbitMQ] Erro ao encerrar conexão: {Erro}", ex.Message);
            }

            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
