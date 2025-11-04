namespace Estoque.Application.Interfaces
{
    public interface IRabbitMqPublisher
    {
        public Task PublicarAsync<T>(string queueName, T mensagemObj);
        public ValueTask DisposeAsync();
    }
}