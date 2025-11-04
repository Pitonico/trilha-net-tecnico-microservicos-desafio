using Shared.Enums;

namespace APIs.Vendas.Application.DTOs
{
    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
        public StatusPedidoEnum Status { get; set; } = StatusPedidoEnum.Pendente;
        public List<ItemPedidoResponseDTO> Itens { get; set; } = new();

        public decimal Total { get; init; }
    }
}