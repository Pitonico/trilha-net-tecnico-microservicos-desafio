namespace APIs.Vendas.Application.Messaging.Events
{
    public record PedidoCriadoEvent(int PedidoId, List<ItemPedidoEvent> Itens);
}