using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using APIs.Gateway.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace APIs.Gateway.Controllers.Pedidos
{
    [ApiController]
    [Route("api/vendas/pedidos")]
    [Authorize]
    public class PedidoController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        private readonly string? _pedidosServiceUrl;

        public PedidoController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;

            _pedidosServiceUrl = _config.GetValue<string>("Services:Vendas:Pedidos");
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url, string? content = null)
        {
            HttpRequestMessage request = new(method, url);
            ClaimsPrincipal? user = null;

            string authHeader = Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader.Replace("Bearer ", "");
                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken jwt = handler.ReadJwtToken(token);

                IEnumerable<Claim> claims = [.. jwt.Claims
                    .Where(c => c.Type == ClaimTypes.Name || c.Type == ClaimTypes.Role)
                    .Select(c => new Claim(c.Type, c.Value))
                ];

                user = new ClaimsPrincipal(new ClaimsIdentity(claims));
            }

            string gatewayToken = GatewayTokenGenerator.Generate(_config, user);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", gatewayToken);

            if (content != null)
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            return request;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPedido(
            [FromBody] PedidoRequestDTO dto,
            [FromServices] IValidator<PedidoRequestDTO> validator)
        {
            if (string.IsNullOrEmpty(_pedidosServiceUrl))
                return StatusCode(500, "URL do serviço Produtos não configurada.");

            ValidationResult validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            HttpClient client = _httpClientFactory.CreateClient();

            string jsonContent = JsonSerializer.Serialize(dto);

            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Post, _pedidosServiceUrl, jsonContent);

            HttpResponseMessage response = await client.SendAsync(request);

            return new ContentResult
            {
                Content = await response.Content.ReadAsStringAsync(),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
        
        [HttpGet]
        public async Task<IActionResult> ObterTodosPedidos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(_pedidosServiceUrl))
                return StatusCode(500, "URL do serviço Produtos não configurada.");

            HttpClient client = _httpClientFactory.CreateClient();
                
            string url = $"{_pedidosServiceUrl}?pageNumber={pageNumber}&pageSize={pageSize}";

            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage response = await client.SendAsync(request);

            return new ContentResult
            {
                Content = await response.Content.ReadAsStringAsync(),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPedidoPorId([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(_pedidosServiceUrl))
                return StatusCode(500, "URL do serviço Produtos não configurada.");

            HttpClient client = _httpClientFactory.CreateClient();

            string url = $"{_pedidosServiceUrl}/{id}";

            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Get, url);

            HttpResponseMessage response = await client.SendAsync(request);

            return new ContentResult
            {
                Content = await response.Content.ReadAsStringAsync(),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> AtualizarStatus(
            [FromRoute] string id,
            [FromBody] AtualizarStatusDTO dto)
        {
            if (string.IsNullOrEmpty(_pedidosServiceUrl))
                return StatusCode(500, "URL do serviço Produtos não configurada.");

            HttpClient client = _httpClientFactory.CreateClient();

            string jsonContent = JsonSerializer.Serialize(dto);

            string url = $"{_pedidosServiceUrl}/{id}/status";

            HttpRequestMessage request = CreateRequestMessage(HttpMethod.Put, url, jsonContent);

            HttpResponseMessage response = await client.SendAsync(request);

            return new ContentResult
            {
                Content = await response.Content.ReadAsStringAsync(),
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                StatusCode = (int)response.StatusCode
            };
        }
    }
}