using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrevisaoFaturamento.Application.Unidades;
using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Api.Controllers;

[ApiController]
[Route("api/unidades")]
[Authorize] // Exige o JWT do Gestor
public class UnidadesController : ControllerBase
{
    private readonly CriarUnidadeUseCase _criarUnidade;
    private readonly ListarUnidadesUseCase _listarUnidades;
    private readonly DesativarUnidadeUseCase _desativarUnidade;

    public UnidadesController(
        CriarUnidadeUseCase criarUnidade,
        ListarUnidadesUseCase listarUnidades,
        DesativarUnidadeUseCase desativarUnidade)
    {
        _criarUnidade = criarUnidade;
        _listarUnidades = listarUnidades;
        _desativarUnidade = desativarUnidade;
    }

    private Guid ObterGestorId()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(claimId!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnidadeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarUnidadeRequest request, CancellationToken cancellationToken)
    {
        var result = await _criarUnidade.ExecutarAsync(ObterGestorId(), request, cancellationToken);
        
        if (result.IsFailure) 
            return ToProblemResult(result);

        return CreatedAtAction(nameof(Listar), new { id = result.Data!.Id }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UnidadeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _listarUnidades.ExecutarAsync(ObterGestorId(), cancellationToken);
        return Ok(result.Data);
    }

    [HttpPatch("{id:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken cancellationToken)
    {
        var result = await _desativarUnidade.ExecutarAsync(id, ObterGestorId(), cancellationToken);
        
        if (result.IsFailure) 
            return ToProblemResult(result);

        return NoContent();
    }

    private IActionResult ToProblemResult(Result result)
    {
        var statusCode = result.TipoErro switch
        {
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            TipoErro.Validacao => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(title: "Falha na requisição", detail: result.ErrorMessage, statusCode: statusCode);
    }
}