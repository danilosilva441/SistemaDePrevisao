using PrevisaoFaturamento.Domain.Shared;
using PrevisaoFaturamento.Domain.Unidades;

namespace PrevisaoFaturamento.Application.Unidades;

public class CriarUnidadeUseCase
{
    private readonly IUnidadeRepository _repository;

    public CriarUnidadeUseCase(IUnidadeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UnidadeResponse>> ExecutarAsync(
        Guid gestorId, 
        CriarUnidadeRequest request, 
        CancellationToken cancellationToken = default)
    {
        if (await _repository.NomeJaExisteAsync(gestorId, request.Nome, cancellationToken: cancellationToken))
            return Result<UnidadeResponse>.Failure("Você já possui uma unidade cadastrada com este nome.", TipoErro.Conflito);

        var unidadeResult = Unidade.Criar(
            gestorId, 
            request.Nome, 
            request.Descricao, 
            request.Endereco, 
            request.MetaMensal, 
            request.DataInicio, 
            request.DataFim);

        if (unidadeResult.IsFailure)
            return Result<UnidadeResponse>.Failure(unidadeResult.ErrorMessage!, TipoErro.Validacao);

        var unidade = unidadeResult.Data!;
        await _repository.AdicionarAsync(unidade, cancellationToken);

        return Result<UnidadeResponse>.Success(MapearParaResponse(unidade));
    }

    public static UnidadeResponse MapearParaResponse(Unidade u) => new(
        u.Id, 
        u.Nome, 
        u.Descricao, 
        u.Endereco, 
        u.MetaMensal.ValorReais, 
        u.ProjecaoFaturamento.ValorReais,
        0m, // TODO Fase 3: Somar faturamentos do mês vigente
        0m, // TODO Fase 3: Calcular (Acumulado / Meta) * 100
        u.DataInicio, 
        u.DataFim, 
        u.IsAtivo);
}

public class ListarUnidadesUseCase
{
    private readonly IUnidadeRepository _repository;

    public ListarUnidadesUseCase(IUnidadeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<UnidadeResponse>>> ExecutarAsync(
        Guid gestorId, 
        CancellationToken cancellationToken = default)
    {
        var unidades = await _repository.ListarPorGestorAsync(gestorId, cancellationToken);
        var response = unidades.Select(CriarUnidadeUseCase.MapearParaResponse).ToList();
        
        return Result<IReadOnlyList<UnidadeResponse>>.Success(response);
    }
}

public class DesativarUnidadeUseCase
{
    private readonly IUnidadeRepository _repository;

    public DesativarUnidadeUseCase(IUnidadeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ExecutarAsync(
        Guid id, 
        Guid gestorId, 
        CancellationToken cancellationToken = default)
    {
        var unidade = await _repository.ObterPorIdAsync(id, gestorId, cancellationToken);
        if (unidade is null)
            return Result.Failure("Unidade não encontrada.", TipoErro.NaoEncontrado);

        var desativarResult = unidade.Desativar();
        if (desativarResult.IsFailure)
            return Result.Failure(desativarResult.ErrorMessage!, TipoErro.Validacao);

        await _repository.AtualizarAsync(unidade, cancellationToken);
        return Result.Success();
    }
}