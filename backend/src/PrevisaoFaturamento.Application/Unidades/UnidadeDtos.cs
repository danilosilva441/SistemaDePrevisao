namespace PrevisaoFaturamento.Application.Unidades;

public record CriarUnidadeRequest(
    string Nome, 
    string? Descricao, 
    string? Endereco, 
    decimal MetaMensal, 
    DateOnly? DataInicio, 
    DateOnly? DataFim);

public record AtualizarUnidadeRequest(
    string Nome, 
    string? Descricao, 
    string? Endereco, 
    decimal MetaMensal, 
    DateOnly DataInicio, 
    DateOnly? DataFim);

public record UnidadeResponse(
    Guid Id, 
    string Nome, 
    string? Descricao, 
    string? Endereco, 
    decimal MetaMensal, 
    decimal ProjecaoFaturamento, 
    decimal FaturamentoAcumuladoMes, // Novo: Para a barra de progresso
    decimal PercentualAtingido,      // Novo: Para a barra de progresso
    DateOnly DataInicio, 
    DateOnly? DataFim, 
    bool IsAtivo);