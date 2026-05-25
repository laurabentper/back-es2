using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CardioTrack.Api.Comum;

/// <summary>
/// Atalhos para extrair a identidade do usuario autenticado a partir das claims
/// do token JWT, evitando que cada controller repita a leitura da claim "sub".
/// </summary>
public static class UsuarioAutenticadoExtensoes
{
    /// <summary>
    /// Obtem o identificador do usuario autenticado a partir da claim "sub".
    /// Lanca <see cref="InvalidOperationException"/> quando o token nao traz um
    /// identificador valido, o que so ocorreria com um token malformado em uma
    /// rota ja protegida por <c>[Authorize]</c>.
    /// </summary>
    public static Guid ObterUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? usuario.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(valor, out var id)
            ? id
            : throw new InvalidOperationException(
                "O token nao contem um identificador de usuario valido.");
    }
}
