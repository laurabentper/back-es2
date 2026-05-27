using System.Net;
using System.Net.Http.Json;
using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Tests.Integration.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Integration;

public class TestesDeUsuarios : TesteDeIntegracao
{
    public TestesDeUsuarios(FabricaDeApi fabrica) : base(fabrica)
    {
    }

    [Fact]
    public async Task Cadastrar_ComDadosValidos_Retorna201ComUsuarioSemSenha()
    {
        var email = EmailUnico();

        var resposta = await CadastrarAsync(NovoCadastro(email));

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var usuario = await resposta.Content.ReadFromJsonAsync<UsuarioResposta>(OpcoesJson);
        usuario!.Email.Should().Be(email);
        usuario.Id.Should().NotBe(Guid.Empty);

        // A senha (nem o hash) nunca deve aparecer no corpo da resposta.
        var conteudo = await resposta.Content.ReadAsStringAsync();
        conteudo.Should().NotContainAny("senha", "Senha", "hash", "Hash");
    }

    [Fact]
    public async Task Cadastrar_ComEmailDuplicado_Retorna409()
    {
        var email = EmailUnico();
        (await CadastrarAsync(NovoCadastro(email))).EnsureSuccessStatusCode();

        var resposta = await CadastrarAsync(NovoCadastro(email));

        resposta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cadastrar_ComSenhaCurta_Retorna400()
    {
        var requisicao = NovoCadastro(EmailUnico(), senha: "123");

        var resposta = await CadastrarAsync(requisicao);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200ComToken()
    {
        var email = EmailUnico();
        (await CadastrarAsync(NovoCadastro(email))).EnsureSuccessStatusCode();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/usuarios/login",
            new AutenticarUsuarioRequisicao(email, "senhaForte123"),
            OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var autenticacao = await resposta.Content.ReadFromJsonAsync<AutenticacaoResposta>(OpcoesJson);
        autenticacao!.Token.Should().NotBeNullOrWhiteSpace();
        autenticacao.Usuario.Email.Should().Be(email);
        autenticacao.ExpiraEm.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var email = EmailUnico();
        (await CadastrarAsync(NovoCadastro(email))).EnsureSuccessStatusCode();

        var resposta = await Cliente.PostAsJsonAsync(
            "/api/usuarios/login",
            new AutenticarUsuarioRequisicao(email, "senhaErrada"),
            OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComUsuarioInexistente_Retorna401()
    {
        var resposta = await Cliente.PostAsJsonAsync(
            "/api/usuarios/login",
            new AutenticarUsuarioRequisicao(EmailUnico(), "senhaForte123"),
            OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
