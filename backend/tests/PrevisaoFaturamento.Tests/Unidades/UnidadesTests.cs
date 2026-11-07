using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrevisaoFaturamento.Application.Identidade;
using PrevisaoFaturamento.Application.Unidades;
using PrevisaoFaturamento.Infrastructure.Persistence;

namespace PrevisaoFaturamento.Tests.Unidades;

public class UnidadesTests
{
    [Fact]
    public async Task FluxoCompletoUnidades_Criar_Duplicar_Listar_DeveFuncionarCorretamente()
    {
        // 1. Setup do Banco em Memória Isolado
        await using var conexaoSqlite = new SqliteConnection("Data Source=:memory:");
        await conexaoSqlite.OpenAsync();

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Jwt:Key", "ChaveDeTesteIntegracaoSeguraComMaisDe32Caracteres2026_Unidades");
                builder.UseSetting("Jwt:Issuer", "PrevisaoFaturamento.Api");
                builder.UseSetting("Jwt:Audience", "PrevisaoFaturamento.Web");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor is not null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(conexaoSqlite));

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

        using var client = factory.CreateClient();

        // 2. Registrar um Gestor para pegar o Token JWT
        var registroRequest = new RegistrarUsuarioRequest("Gestor Unidades", "gestor.unidades@teste.com", "123456");
        var registroResp = await client.PostAsJsonAsync("/api/auth/registro", registroRequest);
        var authData = await registroResp.Content.ReadFromJsonAsync<AuthResponse>();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData!.Token);

        // 3. Criar Unidade -> Deve retornar 201 Created
        var criarUnidadeReq = new CriarUnidadeRequest(
            "Unidade Centro", 
            "Estacionamento Matriz", 
            "Rua A, 123", 
            50000m, 
            DateOnly.FromDateTime(DateTime.UtcNow), 
            null);

        var criarResp = await client.PostAsJsonAsync("/api/unidades", criarUnidadeReq);
        Assert.Equal(HttpStatusCode.Created, criarResp.StatusCode);

        var unidadeCriada = await criarResp.Content.ReadFromJsonAsync<UnidadeResponse>();
        Assert.Equal("Unidade Centro", unidadeCriada!.Nome);
        Assert.Equal(50000m, unidadeCriada.MetaMensal);
        Assert.True(unidadeCriada.IsAtivo);

        // 4. Criar Unidade Duplicada (Mesmo Nome) -> Deve retornar 409 Conflict
        var duplicadaResp = await client.PostAsJsonAsync("/api/unidades", criarUnidadeReq);
        Assert.Equal(HttpStatusCode.Conflict, duplicadaResp.StatusCode);

        // 5. Listar Unidades -> Deve retornar 200 OK com 1 unidade na lista
        var listarResp = await client.GetAsync("/api/unidades");
        Assert.Equal(HttpStatusCode.OK, listarResp.StatusCode);

        var lista = await listarResp.Content.ReadFromJsonAsync<List<UnidadeResponse>>();
        Assert.Single(lista!);
        Assert.Equal("Unidade Centro", lista![0].Nome);
    }
}