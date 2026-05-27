using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Dominio.Comum;

public class EntidadeTests
{
    [Fact]
    public void NovaEntidade_RecebeIdentificadorNaoVazio()
    {
        var usuario = DadosDeTeste.NovoUsuario();

        usuario.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Equals_QuandoMesmoTipoEMesmoId_RetornaVerdadeiro()
    {
        var usuario = DadosDeTeste.NovoUsuario();
        var mesmaReferencia = usuario;

        usuario.Equals(mesmaReferencia).Should().BeTrue();
        usuario.GetHashCode().Should().Be(mesmaReferencia.GetHashCode());
    }

    [Fact]
    public void Equals_QuandoIdsDiferentes_RetornaFalso()
    {
        var primeiro = DadosDeTeste.NovoUsuario();
        var segundo = DadosDeTeste.NovoUsuario();

        primeiro.Equals(segundo).Should().BeFalse();
    }

    [Fact]
    public void Equals_QuandoTiposDiferentesComMesmoId_RetornaFalso()
    {
        var usuario = DadosDeTeste.NovoUsuario();
        var medicao = DadosDeTeste.NovaMedicao();

        usuario.Equals(medicao).Should().BeFalse();
    }
}
