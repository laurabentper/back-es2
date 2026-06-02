using System.Net;
using System.Net.Http.Json;
using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Tests.Integration.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Integration;

public class TestesDeMedicoes : TesteDeIntegracao
{
    public TestesDeMedicoes(FabricaDeApi fabrica) : base(fabrica)
    {
    }

    private static RegistrarMedicaoRequisicao MedicaoValida(bool faltaDeAr = false) =>
        new(
            PressaoSistolica: 120,
            PressaoDiastolica: 80,
            FrequenciaCardiaca: 70,
            OxigenacaoSangue: 98,
            PesoCorporal: 72.5m,
            FaltaDeAr: faltaDeAr,
            DorNoPeito: false,
            Tontura: false,
            RegistradaEm: null);

    private static AtualizarMedicaoRequisicao AtualizacaoValida(bool dorNoPeito = false) =>
        new(
            PressaoSistolica: 130,
            PressaoDiastolica: 85,
            FrequenciaCardiaca: 75,
            OxigenacaoSangue: 97,
            PesoCorporal: 71.0m,
            FaltaDeAr: false,
            DorNoPeito: dorNoPeito,
            Tontura: false,
            RegistradaEm: null);

    private async Task<MedicaoResposta> RegistrarMedicaoAsync(HttpClient cliente)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/medicoes", MedicaoValida(), OpcoesJson);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<MedicaoResposta>(OpcoesJson))!;
    }

    [Fact]
    public async Task Registrar_SemToken_Retorna401()
    {
        var resposta = await Cliente.PostAsJsonAsync("/api/medicoes", MedicaoValida(), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Registrar_ComToken_Retorna201EVinculaAoUsuarioAutenticado()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);

        var resposta = await cliente.PostAsJsonAsync("/api/medicoes", MedicaoValida(faltaDeAr: true), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var medicao = await resposta.Content.ReadFromJsonAsync<MedicaoResposta>(OpcoesJson);
        medicao!.UsuarioId.Should().Be(autenticacao.Usuario.Id);
        medicao.PressaoSistolica.Should().Be(120);
        medicao.Sintomas.FaltaDeAr.Should().BeTrue();
        medicao.PossuiSintomas.Should().BeTrue();
    }

    [Fact]
    public async Task Registrar_ComDadosInvalidos_Retorna400()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);
        var invalida = MedicaoValida() with { PressaoSistolica = 70, PressaoDiastolica = 90 };

        var resposta = await cliente.PostAsJsonAsync("/api/medicoes", invalida, OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Atualizar_ComToken_Retorna200ComValoresAtualizados()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);
        var medicao = await RegistrarMedicaoAsync(cliente);

        var resposta = await cliente.PutAsJsonAsync(
            $"/api/medicoes/{medicao.Id}", AtualizacaoValida(dorNoPeito: true), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizada = await resposta.Content.ReadFromJsonAsync<MedicaoResposta>(OpcoesJson);
        atualizada!.Id.Should().Be(medicao.Id);
        atualizada.PressaoSistolica.Should().Be(130);
        atualizada.Sintomas.DorNoPeito.Should().BeTrue();
        atualizada.PossuiSintomas.Should().BeTrue();
    }

    [Fact]
    public async Task Atualizar_MedicaoInexistente_Retorna404()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);

        var resposta = await cliente.PutAsJsonAsync(
            $"/api/medicoes/{Guid.NewGuid()}", AtualizacaoValida(), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_MedicaoDeOutroUsuario_Retorna404()
    {
        var dono = await CadastrarEAutenticarAsync();
        var medicao = await RegistrarMedicaoAsync(ClienteAutenticado(dono.Token));

        var outro = await CadastrarEAutenticarAsync();
        var clienteOutro = ClienteAutenticado(outro.Token);

        var resposta = await clienteOutro.PutAsJsonAsync(
            $"/api/medicoes/{medicao.Id}", AtualizacaoValida(), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Atualizar_SemToken_Retorna401()
    {
        var resposta = await Cliente.PutAsJsonAsync(
            $"/api/medicoes/{Guid.NewGuid()}", AtualizacaoValida(), OpcoesJson);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Remover_ComToken_Retorna204EImpedeNovaAtualizacao()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);
        var medicao = await RegistrarMedicaoAsync(cliente);

        var resposta = await cliente.DeleteAsync($"/api/medicoes/{medicao.Id}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var aposRemocao = await cliente.PutAsJsonAsync(
            $"/api/medicoes/{medicao.Id}", AtualizacaoValida(), OpcoesJson);
        aposRemocao.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_MedicaoDeOutroUsuario_Retorna404()
    {
        var dono = await CadastrarEAutenticarAsync();
        var medicao = await RegistrarMedicaoAsync(ClienteAutenticado(dono.Token));

        var outro = await CadastrarEAutenticarAsync();
        var clienteOutro = ClienteAutenticado(outro.Token);

        var resposta = await clienteOutro.DeleteAsync($"/api/medicoes/{medicao.Id}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remover_SemToken_Retorna401()
    {
        var resposta = await Cliente.DeleteAsync($"/api/medicoes/{Guid.NewGuid()}");

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
