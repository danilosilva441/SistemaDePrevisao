using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrevisaoFaturamento.Application.Identidade;
using PrevisaoFaturamento.Domain.Identidade;
using PrevisaoFaturamento.Infrastructure.Persistence;

namespace PrevisaoFaturamento.Tests.Identidade;

public class IdentidadeTests
{
    // --- 1. TESTE DE ARQUITETURA (Fase 0.6) ---

    [Fact]
    public void Domain_NaoDeveReferenciar_OutrasCamadasDoProjeto()
    {
        var referencias = typeof(Usuario).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("PrevisaoFaturamento.Application", referencias);
        Assert.DoesNotContain("PrevisaoFaturamento.Infrastructure", referencias);
        Assert.DoesNotContain("PrevisaoFaturamento.Api", referencias);
    }

    // --- 2. TESTES DE UNIDADE DO DOMÍNIO ---

    [Fact]
    public void UsuarioCriar_ComDadosValidos_DeveNormalizarEmailERetornarSucesso()
    {
        var resultado = Usuario.Criar("  Danilo Silva ", "DANILO@Email.com ", "hash_seguro");

        Assert.True(resultado.IsSuccess);
        Assert.Equal("Danilo Silva", resultado.Data!.Nome);
        Assert.Equal("danilo@email.com", resultado.Data.Email);
    }

    [Theory]
    [InlineData("", "danilo@email.com", "hash")]
    [InlineData("Da", "danilo@email.com", "hash")]
    [InlineData("Danilo", "email-invalido", "hash")]
    [InlineData("Danilo", "danilo@email.com", "")]
    public void UsuarioCriar_ComDadosInvalidos_DeveRetornarFalha(string nome, string email, string hash)
    {
        var resultado = Usuario.Criar(nome, email, hash);

        Assert.True(resultado.IsFailure);
    }

    // --- 3. TESTES DE INTEGRAÇÃO DOS ENDPOINTS (SQLite In-Memory) ---

    [Fact]
    public async Task FluxoCompletoAuth_RegistroDuplicidadeLoginEMe_DeveFuncionarCorretamente()
    {
        await using var conexaoSqlite = new SqliteConnection("Data Source=:memory:");
        await conexaoSqlite.OpenAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Jwt:Key", "ChaveDeTesteIntegracaoSeguraComMaisDe32Caracteres2026");
                builder.UseSetting("Jwt:Issuer", "PrevisaoFaturamento.Api");
                builder.UseSetting("Jwt:Audience", "PrevisaoFaturamento.Web");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(conexaoSqlite));

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

        using var client = factory.CreateClient();

        // 1. Registro válido -> 201 Created
        var registroRequest = new RegistrarUsuarioRequest("Danilo Gestor", "gestor@teste.com", "123456");
        var registroResp = await client.PostAsJsonAsync("/api/auth/registro", registroRequest);
        Assert.Equal(HttpStatusCode.Created, registroResp.StatusCode);

        var authData = await registroResp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authData);
        Assert.False(string.IsNullOrWhiteSpace(authData!.Token));

        // 2. Registro com e-mail duplicado -> 409 Conflict
        var duplicadoResp = await client.PostAsJsonAsync("/api/auth/registro", registroRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicadoResp.StatusCode);

        // 3. Login com senha errada -> 401 Unauthorized
        var loginErradoResp = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("gestor@teste.com", "senha_errada"));
        Assert.Equal(HttpStatusCode.Unauthorized, loginErradoResp.StatusCode);

        // 4. Consulta /api/auth/me com o Bearer Token -> 200 OK
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.Token);
        var meResp = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);

        var usuarioLogado = await meResp.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.Equal("gestor@teste.com", usuarioLogado!.Email);
    }
}