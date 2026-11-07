using Microsoft.EntityFrameworkCore;
using PrevisaoFaturamento.Domain.Faturamentos;
using PrevisaoFaturamento.Infrastructure.Persistence;
using PrevisaoFaturamento.Domain.Identidade;
using PrevisaoFaturamento.Domain.Unidades;


namespace PrevisaoFaturamento.Infrastructure.Faturamentos;

public class FaturamentoDiarioRepository : IFaturamentoDiarioRepository
{
    private readonly AppDbContext _context;

    public FaturamentoDiarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FaturamentoDiario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<FaturamentoDiario>()
            .Include(f => f.FaturamentosParciais)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<FaturamentoDiario?> ObterPorDataAsync(Guid unidadeId, DateOnly data, CancellationToken cancellationToken = default)
    {
        return await _context.Set<FaturamentoDiario>()
            .Include(f => f.FaturamentosParciais)
            .FirstOrDefaultAsync(f => f.UnidadeId == unidadeId && f.Data == data, cancellationToken);
    }

    public async Task<IReadOnlyList<FaturamentoDiario>> ListarPorUnidadeAsync(Guid unidadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<FaturamentoDiario>()
            .Include(f => f.FaturamentosParciais)
            .Where(f => f.UnidadeId == unidadeId)
            .OrderByDescending(f => f.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(FaturamentoDiario faturamento, CancellationToken cancellationToken = default)
    {
        await _context.Set<FaturamentoDiario>().AddAsync(faturamento, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(FaturamentoDiario faturamento, CancellationToken cancellationToken = default)
    {
        _context.Set<FaturamentoDiario>().Update(faturamento);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
