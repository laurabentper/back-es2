using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Domain.Medicoes;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Aplicacao.Medicoes;

public class SintomasRespostaTests
{
    [Fact]
    public void ParaFlags_SemSintomas_RetornaNenhum()
    {
        var sintomas = SintomasResposta.ParaFlags(faltaDeAr: false, dorNoPeito: false, tontura: false);

        sintomas.Should().Be(Sintoma.Nenhum);
    }

    [Fact]
    public void ParaFlags_ComVariosSintomas_CombinaAsFlags()
    {
        var sintomas = SintomasResposta.ParaFlags(faltaDeAr: true, dorNoPeito: false, tontura: true);

        sintomas.Should().Be(Sintoma.FaltaDeAr | Sintoma.Tontura);
    }

    [Fact]
    public void DeFlags_DecompoeAsFlagsEmBooleanos()
    {
        var resposta = SintomasResposta.DeFlags(Sintoma.DorNoPeito | Sintoma.Tontura);

        resposta.FaltaDeAr.Should().BeFalse();
        resposta.DorNoPeito.Should().BeTrue();
        resposta.Tontura.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void ParaFlags_EDeFlags_SaoInversas(bool faltaDeAr, bool dorNoPeito, bool tontura)
    {
        var flags = SintomasResposta.ParaFlags(faltaDeAr, dorNoPeito, tontura);

        var resposta = SintomasResposta.DeFlags(flags);

        resposta.Should().Be(new SintomasResposta(faltaDeAr, dorNoPeito, tontura));
    }
}
