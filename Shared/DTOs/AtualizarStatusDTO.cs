using Shared.Enums;

namespace Shared.DTOs
{
    public class AtualizarStatusDTO
    {
        public StatusPedidoEnum Status { get; set; }

        public override string ToString()
        {
            return $"Status: {Status}";
        }
    }
}