using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Domain.Entities;

namespace APIs.Vendas.Application.Mappers
{
    public static class ItemPedidoMapper
    {
        public static ItemPedidoResponseDTO ParaResponseDTO(ItemPedido itemPedido)
        {
            return new ItemPedidoResponseDTO
            {
                Id = itemPedido.Id,
                ProdutoId = itemPedido.ProdutoId,
                Quantidade = itemPedido.Quantidade,
                PrecoUnitario = itemPedido.PrecoUnitario,
                SubTotal = itemPedido.SubTotal
            };
        }
    }
}