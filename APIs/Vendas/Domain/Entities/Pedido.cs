using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace APIs.Vendas.Domain.Entities
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public StatusPedidoEnum Status { get; set; } = StatusPedidoEnum.Pendente;
        public List<ItemPedido> Itens { get; set; } = new();
        public decimal Total { get; set; }

        public void CalcularValorTotal()
        {
            if (Itens == null || Itens.Count == 0)
                throw new InvalidOperationException("Pedido deve conter ao menos um item para calcular o total.");

            Total = Itens.Sum(item => item.SubTotal);
        }

        public override string ToString()
        {
            string itensStr = string.Join(", ", Itens.Select(i => i.ToString()));

            return $"Pedido(Id={Id}, DataCriacao={DataCriacao:u}, Status={Status}, Itens=[{itensStr}], Total={Total:C})";
        }
    }

}