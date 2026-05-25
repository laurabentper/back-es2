using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Application.Usuarios.Servicos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CardioTrack.Api.Controllers;

/// <summary>
/// Endpoints de conta de usuario: cadastro e autenticacao. Sao publicos, pois
/// representam a porta de entrada para obter um token JWT.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IServicoUsuario _servico;

    public UsuariosController(IServicoUsuario servico)
    {
        _servico = servico;
    }

    /// <summary>Cadastra uma nova conta de usuario.</summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra uma nova conta de usuario.")]
    [ProducesResponseType(typeof(UsuarioResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuarioRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var usuario = await _servico.CadastrarAsync(requisicao, cancellationToken);
        return Created((string?)null, usuario);
    }

    /// <summary>Autentica por e-mail e senha, retornando um token JWT.</summary>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Autentica por e-mail e senha e retorna um token JWT.")]
    [ProducesResponseType(typeof(AutenticacaoResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] AutenticarUsuarioRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var resultado = await _servico.AutenticarAsync(requisicao, cancellationToken);
        return Ok(resultado);
    }
}
