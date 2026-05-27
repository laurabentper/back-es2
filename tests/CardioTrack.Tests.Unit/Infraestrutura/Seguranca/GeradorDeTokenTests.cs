using System.IdentityModel.Tokens.Jwt;
using CardioTrack.Infrastructure.Seguranca;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CardioTrack.Tests.Unit.Infraestrutura.Seguranca;

public class GeradorDeTokenTests
{
    private readonly OpcoesJwt _opcoes = new()
    {
        Issuer = "CardioTrack.Api",
        Audience = "CardioTrack.Client",
        SecretKey = "chave-secreta-de-teste-com-no-minimo-32-bytes-de-tamanho",
        ExpirationMinutes = 60
    };

    private GeradorDeToken CriarGerador() => new(Options.Create(_opcoes));

    [Fact]
    public void Gerar_ProduzTokenComExpiracaoConformeAsOpcoes()
    {
        var gerador = CriarGerador();
        var usuario = DadosDeTeste.NovoUsuario();

        var resultado = gerador.Gerar(usuario);

        resultado.Token.Should().NotBeNullOrWhiteSpace();
        resultado.ExpiraEm.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(_opcoes.ExpirationMinutes), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Gerar_EmbutiAsClaimsDeIdentificacaoDoUsuario()
    {
        var gerador = CriarGerador();
        var usuario = DadosDeTeste.NovoUsuario(email: "maria@exemplo.com");

        var resultado = gerador.Gerar(usuario);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(resultado.Token);
        token.Issuer.Should().Be(_opcoes.Issuer);
        token.Audiences.Should().Contain(_opcoes.Audience);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "maria@exemplo.com");
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == usuario.NomeCompleto);
    }

    [Fact]
    public void Gerar_ProduzJtiUnicoPorChamada()
    {
        var gerador = CriarGerador();
        var usuario = DadosDeTeste.NovoUsuario();

        var primeiro = new JwtSecurityTokenHandler().ReadJwtToken(gerador.Gerar(usuario).Token);
        var segundo = new JwtSecurityTokenHandler().ReadJwtToken(gerador.Gerar(usuario).Token);

        var jtiPrimeiro = primeiro.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jtiSegundo = segundo.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        jtiPrimeiro.Should().NotBe(jtiSegundo);
    }
}
