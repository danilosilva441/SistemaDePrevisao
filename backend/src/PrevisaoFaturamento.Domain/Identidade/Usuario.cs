using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Domain.Identidade;

public class Usuario : Entity
{
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;

    // Construtor vazio exigido pelo EF Core
    private Usuario() { }

    private Usuario(string nome, string email, string senhaHash)
    {
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
    }

    // Factory Method usando o padrão Result<T> para garantir que nunca exista um Usuário inválido em memória
    public static Result<Usuario> Criar(string nome, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
            return Result<Usuario>.Failure("O nome deve ter no mínimo 3 caracteres.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result<Usuario>.Failure("Informe um e-mail válido.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            return Result<Usuario>.Failure("O hash da senha é obrigatório.");

        return Result<Usuario>.Success(new Usuario(nome, email, senhaHash));
    }
}