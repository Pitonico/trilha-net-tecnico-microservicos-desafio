using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using APIs.Vendas.Domain.Entities;
using APIs.Vendas.Infrastructure.Data;
using APIs.Vendas.Infrastructure.Interfaces;

namespace APIs.Vendas.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {

        private readonly VendasDbContexto _contexto;

        public PedidoRepository(VendasDbContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task AtualizarStatus(Pedido pedido, StatusPedidoEnum novoStatus)
        {
            pedido.Status = novoStatus;
            
            await _contexto.SaveChangesAsync();
        }

        public async Task<Pedido> CriarPedido(Pedido pedido)
        {
            _contexto.Pedidos.Add(pedido);

            await _contexto.SaveChangesAsync();

            return pedido;
        }

        public async Task<Pedido?> ObterPedidoProId(int id)
        {
            return await _contexto.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(List<Pedido> Pedido, int TotalItems)> ObterTodosPedidos(int pageNumber, int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Max(pageSize, 1);

            IOrderedQueryable<Pedido> query = _contexto.Pedidos
                .Include(p => p.Itens)
                .AsNoTracking()
                .OrderBy(p => p.Id);

            int totalItems = await query.CountAsync();

            List<Pedido> pedidos = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
           return (pedidos, totalItems);
        }
  }
}