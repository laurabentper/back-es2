using CardioTrack.Api.Comum;
using CardioTrack.Application.Relatorios.Dtos;
using CardioTrack.Application.Relatorios.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CardioTrack.Api.Controllers;

/// <summary>
/// Endpoints de relatorios do usuario autenticado: o historico das medicoes e o
/// resumo agregado para os graficos do front-end. O periodo e opcional; quando
/// ausente, considera todo o historico.
/// </summary>
[ApiController]
[Authorize]
[Route("api/relatorios")]
[Produces("application/json")]
public sealed class RelatoriosController : ControllerBase
{
    private readonly IServicoRelatorio _servico;

    public RelatoriosController(IServicoRelatorio servico)
    {
        _servico = servico;
    }

    /// <summary>Retorna o historico de medicoes do usuario, da mais recente para a mais antiga.</summary>
    [HttpGet("historico")]
    [SwaggerOperation(Summary = "Historico de medicoes do usuario no periodo informado.")]
    [ProducesResponseType(typeof(HistoricoMedicoesResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Historico(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var historico = await _servico.ObterHistoricoAsync(usuarioId, inicio, fim, cancellationToken);
        return Ok(historico);
    }

    /// <summary>Retorna os dados agregados das medicoes do usuario para os graficos de resumo.</summary>
    [HttpGet("resumo")]
    [SwaggerOperation(Summary = "Resumo agregado das medicoes do usuario no periodo informado.")]
    [ProducesResponseType(typeof(ResumoMedicoesResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Resumo(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var resumo = await _servico.ObterResumoAsync(usuarioId, inicio, fim, cancellationToken);
        return Ok(resumo);
    }
}
