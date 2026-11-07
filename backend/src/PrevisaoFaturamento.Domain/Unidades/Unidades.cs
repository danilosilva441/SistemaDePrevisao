using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Domain.Unidades;

public class Unidade : Entity
{
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string? Endereco { get; private set; }
    public Dinheiro MetaMensal { get; private set; }
    public Dinheiro ProjecaoFaturamento { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly? DataFim { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public bool IsAtivo => DataFim is null || DataFim.Value > DateOnly.FromDateTime(DateTime.UtcNow);

    // Construtor vazio para o EF Core
    private Unidade() { }

    private Unidade(
        Guid usuarioId,
        string nome,
        string? descricao,
        string? endereco,
        Dinheiro metaMensal,
        DateOnly dataInicio,
        DateOnly? dataFim)
    {
        UsuarioId = usuarioId;
        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        MetaMensal = metaMensal;
        ProjecaoFaturamento = Dinheiro.Zero;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public static Result<Unidade> Criar(
        Guid usuarioId,
        string nome,
        string? descricao,
        string? endereco,
        decimal metaMensalReais,
        DateOnly? dataInicio = null,
        DateOnly? dataFim = null)
    {
        if (usuarioId == Guid.Empty)
            return Result<Unidade>.Failure("O Gestor responsável pela unidade é obrigatório.");

        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 2)
            return Result<Unidade>.Failure("O nome da unidade deve ter no mínimo 2 caracteres.");

        var metaResult = Dinheiro.DeReais(metaMensalReais);
        if (metaResult.IsFailure)
            return Result<Unidade>.Failure("A meta mensal não pode ser negativa.");

        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (dataFim.HasValue && dataFim.Value < inicio)
            return Result<Unidade>.Failure("A data de encerramento não pode ser anterior à data de início.");

        return Result<Unidade>.Success(new Unidade(
            usuarioId,
            nome,
            descricao,
            endereco,
            metaResult.Data,
            inicio,
            dataFim));
    }

    public Result Atualizar(
        string nome,
        string? descricao,
        string? endereco,
        decimal metaMensalReais,
        DateOnly dataInicio,
        DateOnly? dataFim)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 2)
            return Result.Failure("O nome da unidade deve ter no mínimo 2 caracteres.");

        var metaResult = Dinheiro.DeReais(metaMensalReais);
        if (metaResult.IsFailure)
            return Result.Failure("A meta mensal não pode ser negativa.");

        if (dataFim.HasValue && dataFim.Value < dataInicio)
            return Result.Failure("A data de encerramento não pode ser anterior à data de início.");

        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        MetaMensal = metaResult.Data;
        DataInicio = dataInicio;
        DataFim = dataFim;
        AtualizadoEm = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AtualizarProjecao(decimal projecaoReais)
    {
        var projecaoResult = Dinheiro.DeReais(projecaoReais);
        if (projecaoResult.IsFailure)
            return Result.Failure("O valor da projeção não pode ser negativo.");

        ProjecaoFaturamento = projecaoResult.Data;
        AtualizadoEm = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Desativar()
    {
        if (!IsAtivo)
            return Result.Failure("Esta unidade já está desativada.");

        DataFim = DateOnly.FromDateTime(DateTime.UtcNow);
        AtualizadoEm = DateTime.UtcNow;
        return Result.Success();
    }

    public void Reativar()
    {
        DataFim = null;
        AtualizadoEm = DateTime.UtcNow;
    }
}