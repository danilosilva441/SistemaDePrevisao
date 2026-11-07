namespace PrevisaoFaturamento.Application.Faturamentos;

public record AbrirCaixaRequest(DateOnly Data, decimal FundoDeCaixa, string? Observacoes);
public record AdicionarParcialRequest(decimal Valor, DateTime HoraInicio, DateTime HoraFim, string Origem);
public record FecharCaixaRequest(decimal ValorConferido, string? Observacoes);

public record FaturamentoParcialResponse(
    Guid Id, 
    decimal Valor, 
    DateTime HoraInicio, 
    DateTime HoraFim, 
    string Origem, 
    bool IsAtivo);

public record FaturamentoDiarioResponse(
    Guid Id, 
    Guid UnidadeId, 
    DateOnly Data, 
    string StatusCaixa, 
    decimal FundoDeCaixa, 
    decimal? ValorConferido, 
    decimal TotalParciais, 
    decimal Diferenca, 
    string? Observacoes, 
    DateTime? DataFechamento, 
    IReadOnlyList<FaturamentoParcialResponse> Parciais);