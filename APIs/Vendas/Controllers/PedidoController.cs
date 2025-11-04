using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Application.Interfaces;

namespace APIs.Vendas.Controllers
{
    [Authorize(Policy = "GatewayOnly")]
    [ApiController]
    [Route("api/pedidos")]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(IPedidoService pedidoService, ILogger<PedidoController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido([FromBody] PedidoRequestDTO dto)
        {
            _logger.LogInformation("Recebida solicitação para criar novo pedido: {PedidoRequestDTO}", dto);

            var pedido = await _pedidoService.CriarPedido(dto);

            return CreatedAtAction(nameof(ObterPedidoPorId), new { id = pedido.Id }, pedido);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPedidoPorId([FromRoute] int id)
        {
            _logger.LogInformation("Recebida solicitação para buscar pedido ID {PedidoId}", id);

            PedidoResponseDTO pedido = await _pedidoService.ObterPedidoPorId(id);

            return Ok(pedido);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodosPedidos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Recebida solicitação para listar pedidos - Página {PageNumber}, Tamanho {PageSize}", pageNumber, pageSize);
            
            var pedidos = await _pedidoService.ObterTodosPedidos(pageNumber, pageSize);

            return Ok(pedidos);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatus([FromRoute] int id, [FromBody] AtualizarStatusDTO dto)
        {
             _logger.LogInformation(
                "Recebida solicitação para atualizar status do pedido {Id} para '{Status}'",
                id,
                dto.Status.ToString());

            await _pedidoService.AtualizarStatus(id, dto.Status);
            
            return Ok(new { Message = "Status alterado com sucesso" });
        }
    }
}