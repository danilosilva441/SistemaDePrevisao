using Microsoft.EntityFrameworkCore;
using PrevisaoFaturamento.Domain.Unidades;
using PrevisaoFaturamento.Infrastructure.Persistence;

namespace PrevisaoFaturamento.Infrastructure.Unidades;

public class UnidadeRepository : IUnidadeRepository
{
    private readonly AppDbContext _context;

    public UnidadeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Unidade>> ListarPorGestorAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Unidade>()
            .AsNoTracking()
            .Where(u => u.UsuarioId == usuarioId)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Unidade?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Unidade>()
            .FirstOrDefaultAsync(u => u.Id == id && u.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<bool> NomeJaExisteAsync(Guid usuarioId, string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var nomeNormalizado = nome.Trim().ToLowerInvariant();
        var query = _context.Set<Unidade>()
            .AsNoTracking()
            .Where(u => u.UsuarioId == usuarioId && u.Nome.ToLower() == nomeNormalizado);

        if (ignorarId.HasValue)
        {
            query = query.Where(u => u.Id != ignorarId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        await _context.Set<Unidade>().AddAsync(unidade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _context.Set<Unidade>().Update(unidade);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Unidade unidade, CancellationToken cancellationToken = default)
    {
        _context.Set<Unidade>().Remove(unidade);
        await _context.SaveChangesAsync(cancellationToken);
    }
}