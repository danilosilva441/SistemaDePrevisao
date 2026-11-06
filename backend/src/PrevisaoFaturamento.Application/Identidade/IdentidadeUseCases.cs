using System.Net.Mail;
using PrevisaoFaturamento.Domain.Identidade;
using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Application.Identidade;

public class RegistrarUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly ITokenService _tokenService;

    public RegistrarUsuarioUseCase(
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> ExecutarAsync(
        RegistrarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Result<AuthResponse>.Failure("Dados de registro inválidos.", TipoErro.Validacao);

        if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Trim().Length < 3)
            return Result<AuthResponse>.Failure("O nome deve ter no mínimo 3 caracteres.", TipoErro.Validacao);

        if (!EmailEhValido(request.Email))
            return Result<AuthResponse>.Failure("Informe um e-mail válido.", TipoErro.Validacao);

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            return Result<AuthResponse>.Failure("A senha deve ter no mínimo 6 caracteres.", TipoErro.Validacao);

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        if (await _usuarioRepository.EmailJaExisteAsync(emailNormalizado, cancellationToken))
            return Result<AuthResponse>.Failure("Este e-mail já está cadastrado.", TipoErro.Conflito);

        var senhaHash = _senhaHasher.Hash(request.Senha);
        var usuarioResult = Usuario.Criar(request.Nome, emailNormalizado, senhaHash);

        if (usuarioResult.IsFailure)
            return Result<AuthResponse>.Failure(usuarioResult.ErrorMessage!, TipoErro.Validacao);

        var usuario = usuarioResult.Data!;
        await _usuarioRepository.AdicionarAsync(usuario, cancellationToken);

        var (token, expiraEm) = _tokenService.GerarToken(usuario);
        var response = new AuthResponse(
            token,
            expiraEm,
            new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.CriadoEm));

        return Result<AuthResponse>.Success(response);
    }

    private static bool EmailEhValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}

public class AutenticarUseCase
{
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly ITokenService _tokenService;

    public AutenticarUseCase(
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher,
        ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> ExecutarAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return Result<AuthResponse>.Failure(MensagemCredenciaisInvalidas, TipoErro.NaoAutorizado);

        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null)
            return Result<AuthResponse>.Failure(MensagemCredenciaisInvalidas, TipoErro.NaoAutorizado);

        var senhaValida = _senhaHasher.Verificar(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
            return Result<AuthResponse>.Failure(MensagemCredenciaisInvalidas, TipoErro.NaoAutorizado);

        var (token, expiraEm) = _tokenService.GerarToken(usuario);
        var response = new AuthResponse(
            token,
            expiraEm,
            new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.CriadoEm));

        return Result<AuthResponse>.Success(response);
    }
}

public class ObterUsuarioLogadoUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ObterUsuarioLogadoUseCase(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Result<UsuarioResponse>> ExecutarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Result<UsuarioResponse>.Failure("Gestor não encontrado.", TipoErro.NaoEncontrado);

        return Result<UsuarioResponse>.Success(
            new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.CriadoEm));
    }
}