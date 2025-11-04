using System.Text;
using System.Text.Json;
using Estoque.Application.Interfaces;
using RabbitMQ.Client;

namespace APIs.Estoque.Application.Messaging
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
            if (_connection?.IsOpen == true)
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

                    var props = new BasicProperties { DeliveryMode = DeliveryModes.Persistent };

                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: false,
                        basicProperties: props,
                        body: body
                    );

                    _logger.LogInformation("[RabbitMQ] Mensagem publicada na fila {Queue}", queueName);
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning("Falha ao publicar mensagem ({Retry}/{Max}). Erro: {Erro}", retryCount, maxRetries, ex.Message);
                    if (retryCount >= maxRetries) throw;
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            if (_connection != null) await _connection.DisposeAsync();
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
