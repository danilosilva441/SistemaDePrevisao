using PrevisaoFaturamento.Domain.Identidade;

namespace PrevisaoFaturamento.Application.Identidade;

public interface ISenhaHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string senhaHash);
}

public interface ITokenService
{
    (string Token, int ExpiraEmSegundos) GerarToken(Usuario usuario);
}