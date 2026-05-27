using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Domain.Usuarios;

namespace CardioTrack.Tests.Integration.Comum;

/// <summary>
/// Base das classes de teste de integracao: expoe o <see cref="HttpClient"/> da
/// API e atalhos para os fluxos repetidos (cadastro e login), alem das opcoes de
/// JSON alinhadas as da API (camelCase e enums como texto).
/// </summary>
[Collection(ColecaoDeIntegracao.Nome)]
public abstract class TesteDeIntegracao
{
    protected TesteDeIntegracao(FabricaDeApi fabrica)
    {
        Cliente = fabrica.CreateClient();
    }

    protected HttpClient Cliente { get; }

    protected static JsonSerializerOptions OpcoesJson { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Gera um e-mail unico para manter cada teste isolado no banco compartilhado.</summary>
    protected static string EmailUnico() => $"usuario-{Guid.NewGuid():N}@exemplo.com";

    protected static CadastrarUsuarioRequisicao NovoCadastro(string email, string senha = "senhaForte123") =>
        new(
            Nome: "Maria",
            Sobrenome: "Silva",
            Email: email,
            Telefone: "11999998888",
            Senha: senha,
            ConfirmacaoSenha: senha,
            DataNascimento: new DateOnly(1990, 5, 20),
            Sexo: Sexo.Feminino,
            PaisResidencia: "Brasil");

    protected Task<HttpResponseMessage> CadastrarAsync(CadastrarUsuarioRequisicao requisicao) =>
        Cliente.PostAsJsonAsync("/api/usuarios", requisicao, OpcoesJson);

    /// <summary>
    /// Cadastra um usuario e autentica, devolvendo a resposta de login completa
    /// (token e dados do usuario) para os cenarios que exigem rota protegida.
    /// </summary>
    protected async Task<AutenticacaoResposta> CadastrarEAutenticarAsync(string? email = null)
    {
        var enderecoEmail = email ?? EmailUnico();
        const string senha = "senhaForte123";

        var cadastro = await CadastrarAsync(NovoCadastro(enderecoEmail, senha));
        cadastro.EnsureSuccessStatusCode();

        var login = await Cliente.PostAsJsonAsync(
            "/api/usuarios/login",
            new AutenticarUsuarioRequisicao(enderecoEmail, senha),
            OpcoesJson);
        login.EnsureSuccessStatusCode();

        return (await login.Content.ReadFromJsonAsync<AutenticacaoResposta>(OpcoesJson))!;
    }

    /// <summary>Cria um cliente HTTP ja autenticado com o token Bearer informado.</summary>
    protected HttpClient ClienteAutenticado(string token)
    {
        var cliente = Cliente;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
