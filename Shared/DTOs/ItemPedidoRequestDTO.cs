namespace Shared.DTOs
{
    public class ItemPedidoRequestDTO
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }

        public override string ToString()
        {
            return $"ProdutoId='{ProdutoId}', Quantidade={Quantidade}";
        }
    }
}