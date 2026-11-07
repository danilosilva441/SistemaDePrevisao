namespace PrevisaoFaturamento.Domain.Shared;

public readonly record struct Dinheiro(long Centavos) : IComparable<Dinheiro>
{
    public static readonly Dinheiro Zero = new(0);

    public decimal ValorReais => Centavos / 100m;

    public static Result<Dinheiro> DeReais(decimal valorReais)
    {
        if (valorReais < 0)
            return Result<Dinheiro>.Failure("O valor monetário não pode ser negativo.");

        var centavos = (long)Math.Round(valorReais * 100m, MidpointRounding.AwayFromZero);
        return Result<Dinheiro>.Success(new Dinheiro(centavos));
    }

    public static Dinheiro DeCentavos(long centavos) => new(centavos < 0 ? 0 : centavos);

    public Dinheiro Somar(Dinheiro outro) => new(Centavos + outro.Centavos);

    public decimal CalcularPercentual(Dinheiro meta)
    {
        if (meta.Centavos <= 0) return 0m;
        return Math.Round((decimal)Centavos / meta.Centavos * 100m, 2);
    }

    public int CompareTo(Dinheiro other) => Centavos.CompareTo(other.Centavos);

    public override string ToString() => $"R$ {ValorReais:N2}";
}