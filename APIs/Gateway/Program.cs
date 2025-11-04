using APIs.Gateway.Logging;
using APIs.Gateway.MIddlewares;
using APIs.Gateway.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- 🔐 Configuração JWT ---

var key = builder.Configuration["Jwt:Key"]!;
var issuer = builder.Configuration["Jwt:Issuer"]!;
var audience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
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

builder.Services.AddAuthorization();

// =========================
// 🔹 Controllers
// =========================
builder.Services.AddControllers();

// Registra os validators automaticamente
builder.Services.AddValidatorsFromAssemblyContaining<ProdutoRequestDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PedidoRequestDTOValidator>();

// =========================
// 🚀 Registrar IHttpClientFactory
// =========================
builder.Services.AddHttpClient();

// =========================
// 🔹 Swagger com suporte a JWT
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway / Auth", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Cole apenas o token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = CustomConsoleFormatter.Name;
});

var app = builder.Build();

// =========================
// 🔹 Middleware
// =========================
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Production"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/", () => new
{
    Message = "Gestão ApiGateway",
    Swagger = "http://localhost:5000/swagger/index.html"
});

app.Run();
