using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace APIs.Estoque.Application.Messaging
{
    public class RabbitMqConsumer<T> : IAsyncDisposable
    {
        private readonly ILogger<RabbitMqConsumer<T>> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _disposed;

        public RabbitMqConsumer(IConfiguration config, ILogger<RabbitMqConsumer<T>> logger)
        {
            _logger = logger;
            _factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:Host"]!,
                UserName = config["RabbitMQ:UserName"]!,
                Password = config["RabbitMQ:Password"]!,
            };
        }

        public async Task StartConsumingAsync(string queueName, Func<T, Task> handleMessageAsync, CancellationToken ct = default)
        {
            int retryCount = 0;
            const int maxRetries = 10;
            const int retryDelayMs = 3000;

            while (true)
            {
                try
                {
                    _connection = await _factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();

                    await _channel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    _logger.LogInformation("[RabbitMQ] Conexão estabelecida e fila declarada: {Queue}", queueName);
                    break; // saiu do loop quando a conexão foi criada com sucesso
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(
                        "[RabbitMQ] Tentativa {Retry}/{Max} falhou. Erro: {Erro}. Aguardando {Delay}ms...",
                        retryCount, maxRetries, ex.Message, retryDelayMs
                    );

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError("[RabbitMQ] Não foi possível conectar ao RabbitMQ após {Max} tentativas.", maxRetries);
                        throw;
                    }

                    await Task.Delay(retryDelayMs, ct);
                }
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var msgJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<T>(msgJson);

                    if (msg == null)
                    {
                        _logger.LogWarning("[RabbitMQ] Mensagem inválida na fila {Queue}", queueName);
                        return;
                    }

                    _logger.LogInformation("[RabbitMQ] Mensagem recebida na fila {Queue}: {Msg}", queueName, msgJson);
                    await handleMessageAsync(msg);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[RabbitMQ] Erro ao processar mensagem na fila {Queue}", queueName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

            while (!ct.IsCancellationRequested) await Task.Delay(1000, ct);
        }


        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
