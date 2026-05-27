using CardioTrack.Application.Medicoes.Validacoes;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Aplicacao.Medicoes;

public class RegistrarMedicaoValidadorTests
{
    private readonly RegistrarMedicaoValidador _validador = new();

    [Fact]
    public void Validar_ComMedicaoValida_NaoApontaErros()
    {
        var resultado = _validador.Validate(DadosDeTeste.MedicaoValida());

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(49)]
    [InlineData(301)]
    public void Validar_ComPressaoSistolicaForaDoIntervalo_ApontaErro(int sistolica)
    {
        var resultado = _validador.Validate(
            DadosDeTeste.MedicaoValida(pressaoSistolica: sistolica, pressaoDiastolica: 40));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PressaoSistolica");
    }

    [Fact]
    public void Validar_QuandoSistolicaNaoMaiorQueDiastolica_ApontaErro()
    {
        var resultado = _validador.Validate(
            DadosDeTeste.MedicaoValida(pressaoSistolica: 90, pressaoDiastolica: 90));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PressaoSistolica");
    }

    [Theory]
    [InlineData(49)]
    [InlineData(101)]
    public void Validar_ComOxigenacaoForaDoIntervalo_ApontaErro(int oxigenacao)
    {
        var resultado = _validador.Validate(DadosDeTeste.MedicaoValida(oxigenacaoSangue: oxigenacao));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "OxigenacaoSangue");
    }

    [Fact]
    public void Validar_ComPesoForaDoIntervalo_ApontaErro()
    {
        var resultado = _validador.Validate(DadosDeTeste.MedicaoValida(pesoCorporal: 0.1m));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PesoCorporal");
    }

    [Fact]
    public void Validar_ComDataNoFuturo_ApontaErro()
    {
        var resultado = _validador.Validate(
            DadosDeTeste.MedicaoValida(registradaEm: DateTime.UtcNow.AddDays(1)));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "RegistradaEm");
    }

    [Fact]
    public void Validar_ComDataNula_NaoApontaErroNaData()
    {
        var resultado = _validador.Validate(DadosDeTeste.MedicaoValida(registradaEm: null));

        resultado.Errors.Should().NotContain(e => e.PropertyName == "RegistradaEm");
    }
}
