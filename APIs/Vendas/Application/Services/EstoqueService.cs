using System.Net;
using System.Net.Http.Headers;
using APIs.Vendas.Application.DTOs;
using APIs.Vendas.Application.Interfaces;

namespace APIs.Vendas.Application.Services
{
    public class EstoqueService : IEstoqueService
    {

        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EstoqueService> _logger;
        private readonly IConfiguration _config;

        private readonly string? URL_BASE;

        public EstoqueService(HttpClient httpClient, ITokenService tokenService, ILogger<EstoqueService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
            _config = config;

            URL_BASE = _config.GetValue<string>("Services:Estoque:Produtos");
        }

        private void AddAuthorizationHeader()
        {
            var token = _tokenService.GenerateToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ProdutoResponseDTO> ObterProdutoPeloId(int produtoId)
        {
            AddAuthorizationHeader();

            HttpResponseMessage response = await _httpClient.GetAsync($"{URL_BASE}/{produtoId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Produto ID {ProdutoId} não encontrado no serviço de estoque", produtoId);
                    throw new KeyNotFoundException("Produto não encontrado");
                }

                _logger.LogError("Erro ao buscar produto ID {ProdutoId}. Código HTTP: {StatusCode}",
                    produtoId, response.StatusCode);
                    
                throw new HttpRequestException(
                    $"Erro ao buscar o produto. Código: {response.StatusCode}"
                );
            }

            ProdutoResponseDTO produto = await response.Content.ReadFromJsonAsync<ProdutoResponseDTO>()
                ?? throw new InvalidOperationException("Erro ao processar a resposta do produto.");

            _logger.LogInformation("Produto ID {ProdutoId} obtido com sucesso. Estoque atual: {QuantidadeEstoque}",
                produtoId, produto.QuantidadeEstoque);

            return produto;
        }

        public async Task VerificarEstoque(int produtoId, int quantidade)
        {            
            ProdutoResponseDTO produto = await ObterProdutoPeloId(produtoId);

            if (produto.QuantidadeEstoque < quantidade)
            {
                _logger.LogWarning("[EstoqueService] Estoque insuficiente para produto ID {ProdutoId}. Estoque atual: {EstoqueAtual}, Solicitado: {Quantidade}",
                    produtoId, produto.QuantidadeEstoque, quantidade);
                    
                throw new InvalidOperationException(
                    $"Produto {produtoId} sem estoque suficiente. Estoque atual: {produto.QuantidadeEstoque}, solicitado: {quantidade}.");
            }
                
        }
    }
}