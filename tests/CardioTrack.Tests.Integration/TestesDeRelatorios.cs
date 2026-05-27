using System.Net;
using System.Net.Http.Json;
using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Application.Relatorios.Dtos;
using CardioTrack.Tests.Integration.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Integration;

public class TestesDeRelatorios : TesteDeIntegracao
{
    public TestesDeRelatorios(FabricaDeApi fabrica) : base(fabrica)
    {
    }

    private static RegistrarMedicaoRequisicao Medicao(int sistolica, bool faltaDeAr = false) =>
        new(
            PressaoSistolica: sistolica,
            PressaoDiastolica: 80,
            FrequenciaCardiaca: 70,
            OxigenacaoSangue: 98,
            PesoCorporal: 72.5m,
            FaltaDeAr: faltaDeAr,
            DorNoPeito: false,
            Tontura: false,
            RegistradaEm: null);

    [Fact]
    public async Task Historico_SemToken_Retorna401()
    {
        var resposta = await Cliente.GetAsync("/api/relatorios/historico");

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Historico_RetornaApenasMedicoesDoUsuarioAutenticado()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);
        (await cliente.PostAsJsonAsync("/api/medicoes", Medicao(110), OpcoesJson)).EnsureSuccessStatusCode();
        (await cliente.PostAsJsonAsync("/api/medicoes", Medicao(130), OpcoesJson)).EnsureSuccessStatusCode();

        var resposta = await cliente.GetAsync("/api/relatorios/historico");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var historico = await resposta.Content.ReadFromJsonAsync<HistoricoMedicoesResposta>(OpcoesJson);
        historico!.Total.Should().Be(2);
        historico.Medicoes.Should().OnlyContain(m => m.UsuarioId == autenticacao.Usuario.Id);
    }

    [Fact]
    public async Task Resumo_SemMedicoes_RetornaResumoVazio()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);

        var resposta = await cliente.GetAsync("/api/relatorios/resumo");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var resumo = await resposta.Content.ReadFromJsonAsync<ResumoMedicoesResposta>(OpcoesJson);
        resumo!.TotalMedicoes.Should().Be(0);
        resumo.PressaoSistolica.Should().BeNull();
    }

    [Fact]
    public async Task Resumo_ComMedicoes_CalculaAgregadosEFrequenciaDeSintomas()
    {
        var autenticacao = await CadastrarEAutenticarAsync();
        var cliente = ClienteAutenticado(autenticacao.Token);
        (await cliente.PostAsJsonAsync("/api/medicoes", Medicao(100, faltaDeAr: true), OpcoesJson)).EnsureSuccessStatusCode();
        (await cliente.PostAsJsonAsync("/api/medicoes", Medicao(140), OpcoesJson)).EnsureSuccessStatusCode();

        var resposta = await cliente.GetAsync("/api/relatorios/resumo");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var resumo = await resposta.Content.ReadFromJsonAsync<ResumoMedicoesResposta>(OpcoesJson);
        resumo!.TotalMedicoes.Should().Be(2);
        resumo.PressaoSistolica!.Minimo.Should().Be(100m);
        resumo.PressaoSistolica.Maximo.Should().Be(140m);
        resumo.PressaoSistolica.Media.Should().Be(120m);
        resumo.Sintomas.FaltaDeAr.Should().Be(1);
        resumo.Sintomas.ComAlgumSintoma.Should().Be(1);
        resumo.Sintomas.SemSintomas.Should().Be(1);
    }
}
