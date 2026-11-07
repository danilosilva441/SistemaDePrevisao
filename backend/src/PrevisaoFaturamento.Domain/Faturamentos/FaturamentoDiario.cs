using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Domain.Faturamentos;

public class FaturamentoDiario : Entity
{
    public Guid UnidadeId { get; private set; }
    public DateOnly Data { get; private set; }
    public StatusCaixa StatusCaixa { get; private set; }
    public Dinheiro FundoDeCaixa { get; private set; }
    public Dinheiro? ValorConferido { get; private set; }
    public string? Observacoes { get; private set; }
    public Guid? FechadoPorUsuarioId { get; private set; }
    public DateTime? DataFechamento { get; private set; }

    private readonly List<FaturamentoParcial> _faturamentosParciais = [];
    public IReadOnlyCollection<FaturamentoParcial> FaturamentosParciais => _faturamentosParciais.AsReadOnly();

    // Construtor vazio para o EF Core
    private FaturamentoDiario() { }

    private FaturamentoDiario(Guid unidadeId, DateOnly data, Dinheiro fundoDeCaixa, string? observacoes)
    {
        UnidadeId = unidadeId;
        Data = data;
        StatusCaixa = StatusCaixa.Aberto;
        FundoDeCaixa = fundoDeCaixa;
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
    }

    public static Result<FaturamentoDiario> Abrir(Guid unidadeId, DateOnly data, decimal fundoDeCaixaReais, string? observacoes = null)
    {
        var fundoResult = Dinheiro.DeReais(fundoDeCaixaReais);
        if (fundoResult.IsFailure)
            return Result<FaturamentoDiario>.Failure("O fundo de caixa não pode ser negativo.");

        return Result<FaturamentoDiario>.Success(new FaturamentoDiario(unidadeId, data, fundoResult.Data, observacoes));
    }

    public Result AdicionarParcial(decimal valorReais, DateTime horaInicio, DateTime horaFim, string origem)
    {
        if (StatusCaixa != StatusCaixa.Aberto)
            return Result.Failure("Não é possível adicionar lançamentos parciais em um caixa que não está aberto.");

        if (horaInicio >= horaFim)
            return Result.Failure("A hora de início deve ser anterior à hora de fim.");

        var valorResult = Dinheiro.DeReais(valorReais);
        if (valorResult.IsFailure)
            return Result.Failure(valorResult.ErrorMessage!);

        // Validação de sobreposição de horários (Overlap)
        bool temSobreposicao = _faturamentosParciais
            .Where(f => f.IsAtivo)
            .Any(f => horaInicio < f.HoraFim && horaFim > f.HoraInicio);

        if (temSobreposicao)
            return Result.Failure("Já existe um faturamento parcial cadastrado neste intervalo de tempo.");

        var parcial = new FaturamentoParcial(Id, valorResult.Data, horaInicio, horaFim, origem);
        _faturamentosParciais.Add(parcial);

        return Result.Success();
    }

    public Dinheiro CalcularTotalParciaisAtivos()
    {
        long totalCentavos = _faturamentosParciais
            .Where(f => f.IsAtivo)
            .Sum(f => f.Valor.Centavos);
        
        return Dinheiro.DeCentavos(totalCentavos);
    }

    public Result FecharCaixa(Guid usuarioId, decimal valorConferidoReais, string? observacoes = null)
    {
        if (StatusCaixa != StatusCaixa.Aberto)
            return Result.Failure("Este caixa já está fechado ou conferido.");

        var valorConferidoResult = Dinheiro.DeReais(valorConferidoReais);
        if (valorConferidoResult.IsFailure)
            return Result.Failure("O valor conferido não pode ser negativo.");

        ValorConferido = valorConferidoResult.Data;
        StatusCaixa = StatusCaixa.Fechado;
        FechadoPorUsuarioId = usuarioId;
        DataFechamento = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(observacoes))
            Observacoes = string.IsNullOrWhiteSpace(Observacoes) ? observacoes.Trim() : $"{Observacoes} | Fechamento: {observacoes.Trim()}";

        return Result.Success();
    }

    // Calcula a diferença: (Valor Conferido) - (Total dos Parciais)
    public Dinheiro CalcularDiferenca()
    {
        if (StatusCaixa == StatusCaixa.Aberto || !ValorConferido.HasValue)
            return Dinheiro.Zero;

        long totalParciais = CalcularTotalParciaisAtivos().Centavos;
        return Dinheiro.DeCentavos(ValorConferido.Value.Centavos - totalParciais);
    }
}