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
}
