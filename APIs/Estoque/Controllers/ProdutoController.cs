using APIs.Estoque.Application.DTOs;
using APIs.Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace APIs.Estoque.Controllers
{
    [Authorize(Policy = "InternalAccess")]
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;
        private readonly ILogger<ProdutoController> _logger;

        public ProdutoController(IProdutoService produtoService, ILogger<ProdutoController> logger)
        {
            _produtoService = produtoService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarProduto([FromBody] ProdutoRequestDTO produtoRequestDTO)
        {
            _logger.LogInformation("Recebida solicitação para adicionar novo produto: {ProdutoRequestDTO}", produtoRequestDTO);

            ProdutoResponseDTO produto = await _produtoService.AdicionarProduto(produtoRequestDTO);

            return CreatedAtAction(nameof(ObterProdutoPorId), new { id = produto.Id }, produto);
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            _logger.LogInformation("Recebida solicitação para listar produtos - Página {PageNumber}, Tamanho {PageSize}", pageNumber, pageSize);

            var resultado = await _produtoService.ObterTodosProdutos(pageNumber, pageSize);

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterProdutoPorId([FromRoute] int id)
        {
            _logger.LogInformation("Recebida solicitação para buscar produto ID {ProdutoId}", id);

            ProdutoResponseDTO produto = await _produtoService.ObterProdutoPorId(id);

            return Ok(produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(
            [FromRoute] int id,
            [FromBody] ProdutoRequestDTO produtoRequestDTO)
        {
            _logger.LogInformation("Recebida solicitação para atualizar produto ID {ProdutoId}", id);

            await _produtoService.AtualizarProduto(id, produtoRequestDTO);

            return Ok(new { Message = $"Produto {id} atualizado com sucesso" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverProduto([FromRoute] int id)
        {
            _logger.LogInformation("Recebida solicitação para remover produto ID {ProdutoId}", id);

            await _produtoService.RemoverProduto(id);

            return Ok(new { Message = $"Produto {id} removido com sucesso" });
        }
    }
}