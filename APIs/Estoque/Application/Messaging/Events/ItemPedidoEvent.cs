namespace APIs.Estoque.Application.Messaging.Events
{
    public record ItemPedidoEvent(int ProdutoId, int Quantidade);
}