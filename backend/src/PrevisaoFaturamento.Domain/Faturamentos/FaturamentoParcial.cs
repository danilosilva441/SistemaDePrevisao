using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Domain.Faturamentos;

public class FaturamentoParcial : Entity
{
    public Guid FaturamentoDiarioId { get; private set; }
    public Dinheiro Valor { get; private set; }
    public DateTime HoraInicio { get; private set; }
    public DateTime HoraFim { get; private set; }
    public string Origem { get; private set; } = null!;
    public bool IsAtivo { get; private set; }

    // Construtor vazio para o EF Core
    private FaturamentoParcial() { }

    internal FaturamentoParcial(
        Guid faturamentoDiarioId,
        Dinheiro valor,
        DateTime horaInicio,
        DateTime horaFim,
        string origem)
    {
        FaturamentoDiarioId = faturamentoDiarioId;
        Valor = valor;
        HoraInicio = horaInicio;
        HoraFim = horaFim;
        Origem = origem.Trim();
        IsAtivo = true;
    }

    public void Desativar() => IsAtivo = false;
}