using Shared.Enums;
using APIs.Vendas.Domain.Entities;

namespace APIs.Vendas.Infrastructure.Interfaces
{
    public interface IPedidoRepository
    {
        public Task<Pedido> CriarPedido(Pedido pedido);
        public Task<(List<Pedido> Pedido, int TotalItems)> ObterTodosPedidos(int pageNumber, int pageSize);
        public Task<Pedido?> ObterPedidoProId(int id);
        public Task AtualizarStatus(Pedido pedido, StatusPedidoEnum novoStatus);
    }
}