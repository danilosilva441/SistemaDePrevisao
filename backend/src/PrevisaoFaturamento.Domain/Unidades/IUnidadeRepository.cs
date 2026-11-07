namespace PrevisaoFaturamento.Domain.Unidades;

public interface IUnidadeRepository
{
    Task<IReadOnlyList<Unidade>> ListarPorGestorAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Unidade?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<bool> NomeJaExisteAsync(Guid usuarioId, string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default);
    Task RemoverAsync(Unidade unidade, CancellationToken cancellationToken = default);
}