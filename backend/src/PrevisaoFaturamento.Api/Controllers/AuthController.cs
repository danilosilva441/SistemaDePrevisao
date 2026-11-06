using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrevisaoFaturamento.Application.Identidade;
using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegistrarUsuarioUseCase _registrarUsuario;
    private readonly AutenticarUseCase _autenticar;
    private readonly ObterUsuarioLogadoUseCase _obterUsuarioLogado;

    public AuthController(
        RegistrarUsuarioUseCase registrarUsuario,
        AutenticarUseCase autenticar,
        ObterUsuarioLogadoUseCase obterUsuarioLogado)
    {
        _registrarUsuario = registrarUsuario;
        _autenticar = autenticar;
        _obterUsuarioLogado = obterUsuarioLogado;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registrarUsuario.ExecutarAsync(request, cancellationToken);

        if (result.IsFailure)
            return ToProblemResult(result);

        return CreatedAtAction(nameof(Me), result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _autenticar.ExecutarAsync(request, cancellationToken);

        if (result.IsFailure)
            return ToProblemResult(result);

        return Ok(result.Data);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(claimId) || !Guid.TryParse(claimId, out var usuarioId))
        {
            return Problem(
                title: "Não autorizado",
                detail: "Token inválido ou identificação do Gestor ausente.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _obterUsuarioLogado.ExecutarAsync(usuarioId, cancellationToken);

        if (result.IsFailure)
            return ToProblemResult(result);

        return Ok(result.Data);
    }

    private IActionResult ToProblemResult(Result result)
    {
        var statusCode = result.TipoErro switch
        {
            TipoErro.Validacao => StatusCodes.Status400BadRequest,
            TipoErro.NaoAutorizado => StatusCodes.Status401Unauthorized,
            TipoErro.Proibido => StatusCodes.Status403Forbidden,
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            title: "Falha na requisição",
            detail: result.ErrorMessage,
            statusCode: statusCode);
    }
}