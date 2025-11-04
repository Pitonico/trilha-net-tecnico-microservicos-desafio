using Shared;
using Shared.DTOs;
using Shared.Enums;
using APIs.Vendas.Application.DTOs;

namespace APIs.Vendas.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponseDTO> CriarPedido(PedidoRequestDTO pedidoDto);
        Task<PedidoResponseDTO> ObterPedidoPorId(int id);
        Task<PagedResult<PedidoResponseDTO>> ObterTodosPedidos(int pageNumber, int pageSize);
        Task AtualizarStatus(int id, StatusPedidoEnum statusPedidoEnum);
    }
}