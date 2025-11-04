using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using APIs.Vendas.Logging;
using APIs.Vendas.Middlewares;
using APIs.Vendas.Application.Interfaces;
using APIs.Vendas.Application.Messaging;
using APIs.Vendas.Application.Services;
using APIs.Vendas.Infrastructure.Data;
using APIs.Vendas.Infrastructure.Interfaces;
using APIs.Vendas.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configuração do PostgreSQL
builder.Services.AddDbContext<VendasDbContexto>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Serviços e repositórios
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddHttpClient();

// 2️⃣ Autorização
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            RoleClaimType = "role",
            NameClaimType = "unique_name"
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    statusCode = 401,
                    error = "Não autorizado",
                    path = context.Request.Path.Value,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    statusCode = 403,
                    error = "Proibido",
                    path = context.Request.Path.Value,
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("GatewayOnly", policy =>
        policy.RequireClaim("FromGateway", "true"));

// 4️⃣ Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Converte enums para string automaticamente
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Deixa os nomes dos enums em camelCase no JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// 5️⃣ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Serviço de Vendas",
        Version = "v1"
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = CustomConsoleFormatter.Name;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VendasDbContexto>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Production"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7️⃣ Middleware
app.UseHttpsRedirection();

// 9️⃣ Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Serviço de Vendas v1");
});

app.UseAuthentication();
app.UseAuthorization();

// 10️⃣ Endpoints
app.MapControllers();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/", () => new {
    Message = "Serviço de Vendas rodando!",
    Swagger = "http://localhost:5001/swagger/index.html"
}).WithName("Index");

app.Run();
