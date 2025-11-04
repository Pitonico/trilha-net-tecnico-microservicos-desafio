namespace APIs.Vendas.Application.DTOs
{
    public class ItemPedidoResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}