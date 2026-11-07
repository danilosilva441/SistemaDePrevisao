using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrevisaoFaturamento.Application.Faturamentos;
using PrevisaoFaturamento.Domain.Shared;

namespace PrevisaoFaturamento.Api.Controllers;

[ApiController]
[Route("api/unidades/{unidadeId:guid}/faturamentos")]
[Authorize]
public class FaturamentosController : ControllerBase
{
    private readonly FaturamentoUseCases _useCases;

    public FaturamentosController(FaturamentoUseCases useCases)
    {
        _useCases = useCases;
    }

    private Guid ObterGestorId()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(claimId!);
    }

    [HttpGet("{data}")]
    public async Task<IActionResult> Obter(Guid unidadeId, DateOnly data, CancellationToken ct)
    {
        var result = await _useCases.ObterPorDataAsync(unidadeId, data, ObterGestorId(), ct);
        return result.IsSuccess ? Ok(result.Data) : ToProblemResult(result);
    }

    [HttpPost("abrir")]
    public async Task<IActionResult> Abrir(Guid unidadeId, [FromBody] AbrirCaixaRequest request, CancellationToken ct)
    {
        var result = await _useCases.AbrirCaixaAsync(unidadeId, ObterGestorId(), request, ct);
        return result.IsSuccess ? CreatedAtAction(nameof(Obter), new { unidadeId, data = request.Data.ToString("yyyy-MM-dd") }, result.Data) : ToProblemResult(result);
    }

    [HttpPost("{data}/parciais")]
    public async Task<IActionResult> AdicionarParcial(Guid unidadeId, DateOnly data, [FromBody] AdicionarParcialRequest request, CancellationToken ct)
    {
        var result = await _useCases.AdicionarParcialAsync(unidadeId, data, ObterGestorId(), request, ct);
        return result.IsSuccess ? Ok(result.Data) : ToProblemResult(result);
    }

    [HttpPost("{data}/fechar")]
    public async Task<IActionResult> Fechar(Guid unidadeId, DateOnly data, [FromBody] FecharCaixaRequest request, CancellationToken ct)
    {
        var result = await _useCases.FecharCaixaAsync(unidadeId, data, ObterGestorId(), request, ct);
        return result.IsSuccess ? Ok(result.Data) : ToProblemResult(result);
    }

    private IActionResult ToProblemResult(Result result)
    {
        var statusCode = result.TipoErro switch
        {
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
        return Problem(title: "Falha na requisição", detail: result.ErrorMessage, statusCode: statusCode);
    }
}