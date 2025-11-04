using System.Text;
using System.Text.Json;
using APIs.Estoque.Logging;
using APIs.Estoque.Middlewares;
using APIs.Estoque.Application.Interfaces;
using APIs.Estoque.Application.Messaging;
using APIs.Estoque.Application.Messaging.Events;
using APIs.Estoque.Application.Services;
using APIs.Estoque.Infrastructure.Data;
using APIs.Estoque.Infrastructure.Interfaces;
using APIs.Estoque.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Estoque.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Configuração do PostgreSQL
builder.Services.AddDbContext<EstoqueDbContexto>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI RabbitMQ
builder.Services.AddSingleton(typeof(RabbitMqConsumer<>));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

// HostedService para consumer
builder.Services.AddHostedService<PedidoConsumerService>();


// 2️⃣ Serviços e repositórios
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();

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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("InternalAccess", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "FromService" && c.Value == "Vendas") ||
            context.User.HasClaim(c => c.Type == "FromGateway" && c.Value == "true")
        ));

// 4️⃣ Controllers
builder.Services.AddControllers();

// 5️⃣ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Serviço de Estoque",
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
    var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContexto>();
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Serviço de Estoque v1");
});

app.UseAuthentication();
app.UseAuthorization();

// 10️⃣ Endpoints
app.MapControllers();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/", () => new {
    Message = "Serviço de Estoque rodando!",
    Swagger = "http://localhost:5001/swagger/index.html"
}).WithName("Index");

app.Run();
