using PrevisaoFaturamento.Domain.Faturamentos;
using PrevisaoFaturamento.Domain.Shared;
using PrevisaoFaturamento.Domain.Unidades;

namespace PrevisaoFaturamento.Application.Faturamentos;

public class FaturamentoUseCases
{
    private readonly IFaturamentoDiarioRepository _faturamentoRepo;
    private readonly IUnidadeRepository _unidadeRepo;

    public FaturamentoUseCases(IFaturamentoDiarioRepository faturamentoRepo, IUnidadeRepository unidadeRepo)
    {
        _faturamentoRepo = faturamentoRepo;
        _unidadeRepo = unidadeRepo;
    }

    private async Task<Result> ValidarAcessoUnidade(Guid unidadeId, Guid gestorId, CancellationToken ct)
    {
        var unidade = await _unidadeRepo.ObterPorIdAsync(unidadeId, gestorId, ct);
        return unidade is null 
            ? Result.Failure("Unidade não encontrada ou você não tem permissão para acessá-la.", TipoErro.NaoEncontrado) 
            : Result.Success();
    }

    public async Task<Result<FaturamentoDiarioResponse>> AbrirCaixaAsync(Guid unidadeId, Guid gestorId, AbrirCaixaRequest request, CancellationToken ct = default)
    {
        var acessoResult = await ValidarAcessoUnidade(unidadeId, gestorId, ct);
        if (acessoResult.IsFailure) return Result<FaturamentoDiarioResponse>.Failure(acessoResult.ErrorMessage!, acessoResult.TipoErro);

        var existente = await _faturamentoRepo.ObterPorDataAsync(unidadeId, request.Data, ct);
        if (existente is not null)
            return Result<FaturamentoDiarioResponse>.Failure("Já existe um caixa aberto para esta unidade nesta data.", TipoErro.Conflito);

        var abrirResult = FaturamentoDiario.Abrir(unidadeId, request.Data, request.FundoDeCaixa, request.Observacoes);
        if (abrirResult.IsFailure)
            return Result<FaturamentoDiarioResponse>.Failure(abrirResult.ErrorMessage!, TipoErro.Validacao);

        await _faturamentoRepo.AdicionarAsync(abrirResult.Data!, ct);
        return Result<FaturamentoDiarioResponse>.Success(Mapear(abrirResult.Data!));
    }

    public async Task<Result<FaturamentoDiarioResponse>> AdicionarParcialAsync(Guid unidadeId, DateOnly data, Guid gestorId, AdicionarParcialRequest request, CancellationToken ct = default)
    {
        var acessoResult = await ValidarAcessoUnidade(unidadeId, gestorId, ct);
        if (acessoResult.IsFailure) return Result<FaturamentoDiarioResponse>.Failure(acessoResult.ErrorMessage!, acessoResult.TipoErro);

        var faturamento = await _faturamentoRepo.ObterPorDataAsync(unidadeId, data, ct);
        if (faturamento is null)
            return Result<FaturamentoDiarioResponse>.Failure("Caixa não encontrado para a data informada.", TipoErro.NaoEncontrado);

        var addResult = faturamento.AdicionarParcial(request.Valor, request.HoraInicio, request.HoraFim, request.Origem);
        if (addResult.IsFailure)
            return Result<FaturamentoDiarioResponse>.Failure(addResult.ErrorMessage!, TipoErro.Validacao);

        await _faturamentoRepo.AtualizarAsync(faturamento, ct);
        return Result<FaturamentoDiarioResponse>.Success(Mapear(faturamento));
    }

    public async Task<Result<FaturamentoDiarioResponse>> FecharCaixaAsync(Guid unidadeId, DateOnly data, Guid gestorId, FecharCaixaRequest request, CancellationToken ct = default)
    {
        var acessoResult = await ValidarAcessoUnidade(unidadeId, gestorId, ct);
        if (acessoResult.IsFailure) return Result<FaturamentoDiarioResponse>.Failure(acessoResult.ErrorMessage!, acessoResult.TipoErro);

        var faturamento = await _faturamentoRepo.ObterPorDataAsync(unidadeId, data, ct);
        if (faturamento is null)
            return Result<FaturamentoDiarioResponse>.Failure("Caixa não encontrado para a data informada.", TipoErro.NaoEncontrado);

        var fecharResult = faturamento.FecharCaixa(gestorId, request.ValorConferido, request.Observacoes);
        if (fecharResult.IsFailure)
            return Result<FaturamentoDiarioResponse>.Failure(fecharResult.ErrorMessage!, TipoErro.Validacao);

        await _faturamentoRepo.AtualizarAsync(faturamento, ct);
        return Result<FaturamentoDiarioResponse>.Success(Mapear(faturamento));
    }

    public async Task<Result<FaturamentoDiarioResponse>> ObterPorDataAsync(Guid unidadeId, DateOnly data, Guid gestorId, CancellationToken ct = default)
    {
        var acessoResult = await ValidarAcessoUnidade(unidadeId, gestorId, ct);
        if (acessoResult.IsFailure) return Result<FaturamentoDiarioResponse>.Failure(acessoResult.ErrorMessage!, acessoResult.TipoErro);

        var faturamento = await _faturamentoRepo.ObterPorDataAsync(unidadeId, data, ct);
        if (faturamento is null)
            return Result<FaturamentoDiarioResponse>.Failure("Caixa não encontrado.", TipoErro.NaoEncontrado);

        return Result<FaturamentoDiarioResponse>.Success(Mapear(faturamento));
    }

    private static FaturamentoDiarioResponse Mapear(FaturamentoDiario f) => new(
        f.Id, f.UnidadeId, f.Data, f.StatusCaixa.ToString(), 
        f.FundoDeCaixa.ValorReais, f.ValorConferido?.ValorReais, 
        f.CalcularTotalParciaisAtivos().ValorReais, f.CalcularDiferenca().ValorReais, 
        f.Observacoes, f.DataFechamento,
        f.FaturamentosParciais.Select(p => new FaturamentoParcialResponse(
            p.Id, p.Valor.ValorReais, p.HoraInicio, p.HoraFim, p.Origem, p.IsAtivo)).ToList());
}