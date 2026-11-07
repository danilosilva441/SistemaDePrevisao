using Microsoft.EntityFrameworkCore;
using PrevisaoFaturamento.Domain.Identidade;
using PrevisaoFaturamento.Domain.Unidades;
using PrevisaoFaturamento.Domain.Faturamentos;

namespace PrevisaoFaturamento.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<FaturamentoDiario> FaturamentosDiarios => Set<FaturamentoDiario>();
    public DbSet<FaturamentoParcial> FaturamentosParciais => Set<FaturamentoParcial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}