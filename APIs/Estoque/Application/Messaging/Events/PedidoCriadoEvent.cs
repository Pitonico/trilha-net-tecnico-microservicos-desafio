namespace APIs.Estoque.Application.Messaging.Events
{
    public record PedidoCriadoEvent(int PedidoId, List<ItemPedidoEvent> Itens);
}