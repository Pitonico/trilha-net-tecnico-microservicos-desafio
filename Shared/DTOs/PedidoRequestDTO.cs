using Shared.Enums;

namespace Shared.DTOs
{
    public class PedidoRequestDTO
    {
        public List<ItemPedidoRequestDTO> Itens { get; set; } = new List<ItemPedidoRequestDTO>();
        public StatusPedidoEnum Status { get; set; } = StatusPedidoEnum.Pendente;
        
        public override string ToString()
        {
            string itensStr = string.Join(", ", Itens.Select(i => i.ToString()));

            return $"Status={Status}, Itens=[{itensStr}]";
        }
        
    }
}