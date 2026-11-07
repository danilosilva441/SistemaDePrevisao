using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrevisaoFaturamento.Domain.Unidades;

namespace PrevisaoFaturamento.Infrastructure.Unidades;

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("Unidades");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UsuarioId)
            .IsRequired();

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Descricao)
            .HasMaxLength(250);

        builder.Property(u => u.Endereco)
            .HasMaxLength(200);

        // Mapeamento do Value Object Dinheiro (armazenado em centavos como long)
        builder.Property(u => u.MetaMensal)
            .HasConversion(
                dinheiro => dinheiro.Centavos,
                centavos => PrevisaoFaturamento.Domain.Shared.Dinheiro.DeCentavos(centavos))
            .IsRequired();

        builder.Property(u => u.ProjecaoFaturamento)
            .HasConversion(
                dinheiro => dinheiro.Centavos,
                centavos => PrevisaoFaturamento.Domain.Shared.Dinheiro.DeCentavos(centavos))
            .IsRequired();

        builder.Property(u => u.DataInicio)
            .IsRequired();

        builder.Property(u => u.DataFim);

        builder.Property(u => u.CriadoEm)
            .IsRequired();

        builder.Property(u => u.AtualizadoEm);

        // Índice para buscas rápidas por Gestor
        builder.HasIndex(u => u.UsuarioId);
    }
}