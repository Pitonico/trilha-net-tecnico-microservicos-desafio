namespace APIs.Vendas.Application.Messaging.Events
{
    public record ItemPedidoEvent(int ProdutoId, int Quantidade);
}