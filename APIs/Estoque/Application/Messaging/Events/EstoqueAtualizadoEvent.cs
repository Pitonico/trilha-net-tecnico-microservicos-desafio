namespace APIs.Estoque.Application.Messaging.Events
{
    public record EstoqueAtualizadoEvent(int ProdutoId, int Quantidade);
}