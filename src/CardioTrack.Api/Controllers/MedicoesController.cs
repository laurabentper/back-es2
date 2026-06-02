using CardioTrack.Api.Comum;
using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Application.Medicoes.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CardioTrack.Api.Controllers;

/// <summary>
/// Endpoints de registro de medicoes de saude cardiaca. Exigem autenticacao: a
/// medicao e sempre associada ao usuario dono do token, nunca a um id informado
/// no corpo da requisicao.
/// </summary>
[ApiController]
[Authorize]
[Route("api/medicoes")]
[Produces("application/json")]
public sealed class MedicoesController : ControllerBase
{
    private readonly IServicoMedicao _servico;

    public MedicoesController(IServicoMedicao servico)
    {
        _servico = servico;
    }

    /// <summary>Registra uma medicao para o usuario autenticado.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Registra uma medicao para o usuario autenticado.")]
    [ProducesResponseType(typeof(MedicaoResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var medicao = await _servico.RegistrarAsync(usuarioId, requisicao, cancellationToken);
        return Created((string?)null, medicao);
    }

    /// <summary>Atualiza uma medicao do usuario autenticado.</summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza uma medicao do usuario autenticado.")]
    [ProducesResponseType(typeof(MedicaoResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        [FromRoute] Guid id,
        [FromBody] AtualizarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var medicao = await _servico.AtualizarAsync(usuarioId, id, requisicao, cancellationToken);
        return Ok(medicao);
    }

    /// <summary>Remove uma medicao do usuario autenticado.</summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove uma medicao do usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        await _servico.RemoverAsync(usuarioId, id, cancellationToken);
        return NoContent();
    }
}
