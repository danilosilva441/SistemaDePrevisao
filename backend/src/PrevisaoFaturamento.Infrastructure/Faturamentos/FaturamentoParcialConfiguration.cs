using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrevisaoFaturamento.Domain.Faturamentos;

namespace PrevisaoFaturamento.Infrastructure.Faturamentos;

public class FaturamentoParcialConfiguration : IEntityTypeConfiguration<FaturamentoParcial>
{
    public void Configure(EntityTypeBuilder<FaturamentoParcial> builder)
    {
        builder.ToTable("FaturamentosParciais");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FaturamentoDiarioId).IsRequired();

        // Mapeamento do Value Object
        builder.Property(p => p.Valor)
            .HasConversion(
                d => d.Centavos,
                c => PrevisaoFaturamento.Domain.Shared.Dinheiro.DeCentavos(c))
            .IsRequired();

        builder.Property(p => p.HoraInicio).IsRequired();
        builder.Property(p => p.HoraFim).IsRequired();
        builder.Property(p => p.Origem).IsRequired().HasMaxLength(100);
        builder.Property(p => p.IsAtivo).IsRequired();
    }
}