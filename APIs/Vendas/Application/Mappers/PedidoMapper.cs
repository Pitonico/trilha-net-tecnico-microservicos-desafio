using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Domain.Entities;

namespace APIs.Vendas.Application.Mappers
{
    public static class PedidoMapper
    {
        public static PedidoResponseDTO ParaResponseDTO(Pedido pedido)
        {
            return new PedidoResponseDTO
            {
                Id = pedido.Id,
                Data = pedido.DataCriacao,
                Status = pedido.Status,
                Itens = pedido.Itens.Select(ItemPedidoMapper.ParaResponseDTO).ToList(),
                Total = pedido.Total
            };
        }
    }
}