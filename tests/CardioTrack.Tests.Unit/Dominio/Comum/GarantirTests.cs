using CardioTrack.Domain.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Dominio.Comum;

public class GarantirTests
{
    [Fact]
    public void Que_QuandoCondicaoVerdadeira_NaoLanca()
    {
        var acao = () => Garantir.Que(true, "nunca deveria aparecer");

        acao.Should().NotThrow();
    }

    [Fact]
    public void Que_QuandoCondicaoFalsa_LancaComMensagemInformada()
    {
        var acao = () => Garantir.Que(false, "mensagem de erro");

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("mensagem de erro");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NaoVazio_QuandoValorAusente_Lanca(string? valor)
    {
        var acao = () => Garantir.NaoVazio(valor, "campo");

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*campo*");
    }

    [Fact]
    public void NaoVazio_QuandoValorPreenchido_NaoLanca()
    {
        var acao = () => Garantir.NaoVazio("valor", "campo");

        acao.Should().NotThrow();
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(300)]
    public void DentroDoIntervaloInteiro_QuandoNoLimiteOuDentro_NaoLanca(int valor)
    {
        var acao = () => Garantir.DentroDoIntervalo(valor, 50, 300, "campo");

        acao.Should().NotThrow();
    }

    [Theory]
    [InlineData(49)]
    [InlineData(301)]
    public void DentroDoIntervaloInteiro_QuandoForaDoIntervalo_Lanca(int valor)
    {
        var acao = () => Garantir.DentroDoIntervalo(valor, 50, 300, "campo");

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*campo*entre 50 e 300*");
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(500.1)]
    public void DentroDoIntervaloDecimal_QuandoForaDoIntervalo_Lanca(double valor)
    {
        var acao = () => Garantir.DentroDoIntervalo((decimal)valor, 0.5m, 500m, "peso");

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*peso*");
    }
}
