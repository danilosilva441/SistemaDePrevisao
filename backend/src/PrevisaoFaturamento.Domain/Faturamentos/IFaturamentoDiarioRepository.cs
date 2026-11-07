namespace PrevisaoFaturamento.Domain.Faturamentos;

public interface IFaturamentoDiarioRepository
{
    Task<FaturamentoDiario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FaturamentoDiario?> ObterPorDataAsync(Guid unidadeId, DateOnly data, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FaturamentoDiario>> ListarPorUnidadeAsync(Guid unidadeId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(FaturamentoDiario faturamento, CancellationToken cancellationToken = default);
    Task AtualizarAsync(FaturamentoDiario faturamento, CancellationToken cancellationToken = default);
}