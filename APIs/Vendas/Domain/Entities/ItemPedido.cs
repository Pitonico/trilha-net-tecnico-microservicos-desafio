using System.ComponentModel.DataAnnotations.Schema;

namespace APIs.Vendas.Domain.Entities
{
    public class ItemPedido
    {

        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int PedidoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal SubTotal { get; set; }
        public Pedido Pedido { get; set; } = default!;

        public override string ToString()
        {
            return $"ItemPedido(Id={Id}, ProdutoId='{ProdutoId}', Quantidade={Quantidade}, PrecoUnitario={PrecoUnitario}, SubTotal={SubTotal})";
        }
    }
}