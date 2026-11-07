using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrevisaoFaturamento.Domain.Faturamentos;

namespace PrevisaoFaturamento.Infrastructure.Faturamentos;

public class FaturamentoDiarioConfiguration : IEntityTypeConfiguration<FaturamentoDiario>
{
    public void Configure(EntityTypeBuilder<FaturamentoDiario> builder)
    {
        builder.ToTable("FaturamentosDiarios");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.UnidadeId).IsRequired();
        builder.Property(f => f.Data).IsRequired();
        builder.Property(f => f.StatusCaixa).IsRequired();

        // Mapeamento dos Value Objects
        builder.Property(f => f.FundoDeCaixa)
            .HasConversion(
                d => d.Centavos,
                c => PrevisaoFaturamento.Domain.Shared.Dinheiro.DeCentavos(c))
            .IsRequired();

        builder.Property(f => f.ValorConferido)
            .HasConversion(
                d => d!.Value.Centavos,
                c => PrevisaoFaturamento.Domain.Shared.Dinheiro.DeCentavos(c));

        builder.Property(f => f.Observacoes).HasMaxLength(500);

        // Relacionamento (Raiz -> Filhos) com deleção em cascata
        builder.HasMany(f => f.FaturamentosParciais)
            .WithOne()
            .HasForeignKey(p => p.FaturamentoDiarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para garantir que não existam dois fechamentos para a mesma unidade no mesmo dia
        builder.HasIndex(f => new { f.UnidadeId, f.Data }).IsUnique();
    }
}