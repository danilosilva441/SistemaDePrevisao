using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PrevisaoFaturamento.Application.Identidade;
using PrevisaoFaturamento.Domain.Identidade;
using PrevisaoFaturamento.Infrastructure.Identidade;
using PrevisaoFaturamento.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. ProblemDetails (RFC 7807) nativo
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger com suporte a Bearer Token para facilitar testes
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Previsão de Faturamento API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira apenas o token JWT gerado no login/registro."
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

// 3. Banco de Dados SQLite multiplataforma (Fedora: ~/.local/share/PrevisaoFaturamento | Windows: %LocalAppData%\PrevisaoFaturamento)
var pastaDados = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "PrevisaoFaturamento");
Directory.CreateDirectory(pastaDados);

var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? $"Data Source={Path.Combine(pastaDados, "previsao_faturamento.db")}";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(dbPath));

// 4. Injeção de Dependência - Módulo Identidade
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddSingleton<ISenhaHasher, SenhaHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<RegistrarUsuarioUseCase>();
builder.Services.AddScoped<AutenticarUseCase>();
builder.Services.AddScoped<ObterUsuarioLogadoUseCase>();

// 5. Autenticação JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Chave 'Jwt:Key' não configurada. Execute o comando dotnet user-secrets.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PrevisaoFaturamento.Api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PrevisaoFaturamento.Web",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de saúde (Fase 0.4)
app.MapGet("/api/saude", () => Results.Ok(new
{
    Status = "Online",
    Ambiente = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
}));

app.Run();

//Necessário para os testes de integração com WebApplicationFactory
public partial class Program { }