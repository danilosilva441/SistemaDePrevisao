using Microsoft.AspNetCore.Identity;
using PrevisaoFaturamento.Application.Identidade;

namespace PrevisaoFaturamento.Infrastructure.Identidade;

public class SenhaHasher : ISenhaHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object DummyUser = new();

    public string Hash(string senha)
    {
        return _hasher.HashPassword(DummyUser, senha);
    }

    public bool Verificar(string senha, string senhaHash)
    {
        var resultado = _hasher.VerifyHashedPassword(DummyUser, senhaHash, senha);
        return resultado != PasswordVerificationResult.Failed;
    }
}