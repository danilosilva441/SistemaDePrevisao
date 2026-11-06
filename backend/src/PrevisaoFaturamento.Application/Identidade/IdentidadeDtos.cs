namespace PrevisaoFaturamento.Application.Identidade;

public record RegistrarUsuarioRequest(string Nome, string Email, string Senha);

public record LoginRequest(string Email, string Senha);

public record UsuarioResponse(Guid Id, string Nome, string Email, DateTime CriadoEm);

public record AuthResponse(string Token, int ExpiraEmSegundos, UsuarioResponse Usuario);