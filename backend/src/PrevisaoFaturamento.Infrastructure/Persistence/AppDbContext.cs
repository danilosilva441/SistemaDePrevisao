using Microsoft.EntityFrameworkCore;
using PrevisaoFaturamento.Domain.Identidade;

namespace PrevisaoFaturamento.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automaticamente todas as classes que implementam IEntityTypeConfiguration
        // (como a UsuarioConfiguration acima e as futuras Unidades, Metas, etc.)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}