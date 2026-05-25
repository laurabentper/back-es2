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
}
