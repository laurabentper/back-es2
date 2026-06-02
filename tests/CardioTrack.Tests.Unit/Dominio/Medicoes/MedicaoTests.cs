using CardioTrack.Domain.Comum;
using CardioTrack.Domain.Medicoes;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Dominio.Medicoes;

public class MedicaoTests
{
    [Fact]
    public void Construtor_ComDadosValidos_PreencheAsPropriedades()
    {
        var usuarioId = Guid.NewGuid();
        var registradaEm = DateTime.UtcNow.AddHours(-2);

        var medicao = DadosDeTeste.NovaMedicao(
            usuarioId: usuarioId,
            pressaoSistolica: 130,
            pressaoDiastolica: 85,
            frequenciaCardiaca: 72,
            oxigenacaoSangue: 97,
            pesoCorporal: 80.4m,
            sintomas: Sintoma.DorNoPeito | Sintoma.Tontura,
            registradaEm: registradaEm);

        medicao.UsuarioId.Should().Be(usuarioId);
        medicao.PressaoSistolica.Should().Be(130);
        medicao.PressaoDiastolica.Should().Be(85);
        medicao.FrequenciaCardiaca.Should().Be(72);
        medicao.OxigenacaoSangue.Should().Be(97);
        medicao.PesoCorporal.Should().Be(80.4m);
        medicao.Sintomas.Should().Be(Sintoma.DorNoPeito | Sintoma.Tontura);
        medicao.RegistradaEm.Should().Be(registradaEm);
        medicao.CriadaEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construtor_SemDataInformada_UsaAgora()
    {
        var medicao = DadosDeTeste.NovaMedicao(registradaEm: null);

        medicao.RegistradaEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construtor_SemUsuario_Lanca()
    {
        var acao = () => DadosDeTeste.NovaMedicao(usuarioId: Guid.Empty);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*usuario*");
    }

    [Theory]
    [InlineData(49)]
    [InlineData(301)]
    public void Construtor_ComPressaoSistolicaForaDoIntervalo_Lanca(int sistolica)
    {
        var acao = () => DadosDeTeste.NovaMedicao(pressaoSistolica: sistolica, pressaoDiastolica: 40);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*pressaoSistolica*");
    }

    [Fact]
    public void Construtor_QuandoSistolicaNaoMaiorQueDiastolica_Lanca()
    {
        var acao = () => DadosDeTeste.NovaMedicao(pressaoSistolica: 90, pressaoDiastolica: 90);

        acao.Should().Throw<ExcecaoDeDominio>()
            .WithMessage("*sistolica deve ser maior que a diastolica*");
    }

    [Theory]
    [InlineData(19)]
    [InlineData(251)]
    public void Construtor_ComFrequenciaCardiacaForaDoIntervalo_Lanca(int frequencia)
    {
        var acao = () => DadosDeTeste.NovaMedicao(frequenciaCardiaca: frequencia);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*frequenciaCardiaca*");
    }

    [Theory]
    [InlineData(49)]
    [InlineData(101)]
    public void Construtor_ComOxigenacaoForaDoIntervalo_Lanca(int oxigenacao)
    {
        var acao = () => DadosDeTeste.NovaMedicao(oxigenacaoSangue: oxigenacao);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*oxigenacaoSangue*");
    }

    [Fact]
    public void Construtor_ComDataNoFuturo_Lanca()
    {
        var futuro = DateTime.UtcNow.AddMinutes(5);

        var acao = () => DadosDeTeste.NovaMedicao(registradaEm: futuro);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*futuro*");
    }

    [Fact]
    public void PossuiSintomas_QuandoNenhum_RetornaFalso()
    {
        var medicao = DadosDeTeste.NovaMedicao(sintomas: Sintoma.Nenhum);

        medicao.PossuiSintomas.Should().BeFalse();
    }

    [Fact]
    public void PossuiSintomas_QuandoAlgum_RetornaVerdadeiro()
    {
        var medicao = DadosDeTeste.NovaMedicao(sintomas: Sintoma.Tontura);

        medicao.PossuiSintomas.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_ComDadosValidos_AlteraValoresPreservandoUsuarioECriacao()
    {
        var usuarioId = Guid.NewGuid();
        var medicao = DadosDeTeste.NovaMedicao(usuarioId: usuarioId);
        var criadaEm = medicao.CriadaEm;
        var registradaEm = DateTime.UtcNow.AddHours(-1);

        medicao.Atualizar(
            pressaoSistolica: 135,
            pressaoDiastolica: 88,
            frequenciaCardiaca: 80,
            oxigenacaoSangue: 96,
            pesoCorporal: 75.2m,
            sintomas: Sintoma.FaltaDeAr,
            registradaEm: registradaEm);

        medicao.UsuarioId.Should().Be(usuarioId);
        medicao.CriadaEm.Should().Be(criadaEm);
        medicao.PressaoSistolica.Should().Be(135);
        medicao.PressaoDiastolica.Should().Be(88);
        medicao.FrequenciaCardiaca.Should().Be(80);
        medicao.OxigenacaoSangue.Should().Be(96);
        medicao.PesoCorporal.Should().Be(75.2m);
        medicao.Sintomas.Should().Be(Sintoma.FaltaDeAr);
        medicao.RegistradaEm.Should().Be(registradaEm);
    }

    [Fact]
    public void Atualizar_ComValorForaDoIntervalo_Lanca()
    {
        var medicao = DadosDeTeste.NovaMedicao();

        var acao = () => medicao.Atualizar(
            pressaoSistolica: 301,
            pressaoDiastolica: 80,
            frequenciaCardiaca: 70,
            oxigenacaoSangue: 98,
            pesoCorporal: 72.5m,
            sintomas: Sintoma.Nenhum);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*pressaoSistolica*");
    }
}
