using CardioTrack.Application.Abstracoes.Persistencia;
using CardioTrack.Application.Relatorios.Servicos;
using CardioTrack.Domain.Medicoes;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;
using Moq;

namespace CardioTrack.Tests.Unit.Aplicacao.Relatorios;

public class ServicoRelatorioTests
{
    private readonly Mock<IRepositorioMedicao> _repositorio = new();
    private readonly ServicoRelatorio _servico;
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ServicoRelatorioTests()
    {
        _servico = new ServicoRelatorio(_repositorio.Object);
    }

    private void ConfigurarMedicoes(params Medicao[] medicoes) =>
        _repositorio
            .Setup(r => r.ListarPorUsuarioAsync(
                _usuarioId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicoes);

    [Fact]
    public async Task ObterHistoricoAsync_ProjetaMedicoesEInformaOTotal()
    {
        ConfigurarMedicoes(
            DadosDeTeste.NovaMedicao(_usuarioId, sintomas: Sintoma.FaltaDeAr),
            DadosDeTeste.NovaMedicao(_usuarioId));

        var historico = await _servico.ObterHistoricoAsync(_usuarioId);

        historico.Total.Should().Be(2);
        historico.Medicoes.Should().HaveCount(2);
        historico.Medicoes.Should().AllSatisfy(m => m.UsuarioId.Should().Be(_usuarioId));
    }

    [Fact]
    public async Task ObterResumoAsync_SemMedicoes_RetornaResumoVazio()
    {
        ConfigurarMedicoes();

        var resumo = await _servico.ObterResumoAsync(_usuarioId);

        resumo.TotalMedicoes.Should().Be(0);
        resumo.PrimeiraMedicaoEm.Should().BeNull();
        resumo.UltimaMedicaoEm.Should().BeNull();
        resumo.PressaoSistolica.Should().BeNull();
        resumo.PesoCorporal.Should().BeNull();
        resumo.Sintomas.ComAlgumSintoma.Should().Be(0);
        resumo.Sintomas.SemSintomas.Should().Be(0);
    }

    [Fact]
    public async Task ObterResumoAsync_ComMedicoes_CalculaEstatisticasEPeriodo()
    {
        var maisAntiga = DateTime.UtcNow.AddDays(-3);
        var maisRecente = DateTime.UtcNow.AddDays(-1);

        ConfigurarMedicoes(
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 100, registradaEm: maisAntiga),
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 120, registradaEm: DateTime.UtcNow.AddDays(-2)),
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 140, registradaEm: maisRecente));

        var resumo = await _servico.ObterResumoAsync(_usuarioId);

        resumo.TotalMedicoes.Should().Be(3);
        resumo.PrimeiraMedicaoEm.Should().BeCloseTo(maisAntiga, TimeSpan.FromSeconds(1));
        resumo.UltimaMedicaoEm.Should().BeCloseTo(maisRecente, TimeSpan.FromSeconds(1));
        resumo.PressaoSistolica!.Media.Should().Be(120m);
        resumo.PressaoSistolica.Minimo.Should().Be(100m);
        resumo.PressaoSistolica.Maximo.Should().Be(140m);
    }

    [Fact]
    public async Task ObterResumoAsync_ArredondaMediaParaDuasCasas()
    {
        // Sistolicas 100, 100, 101 => media 100,3333... arredondada para 100,33.
        ConfigurarMedicoes(
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 100),
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 100),
            DadosDeTeste.NovaMedicao(_usuarioId, pressaoSistolica: 101));

        var resumo = await _servico.ObterResumoAsync(_usuarioId);

        resumo.PressaoSistolica!.Media.Should().Be(100.33m);
    }

    [Fact]
    public async Task ObterResumoAsync_ContabilizaFrequenciaDeSintomas()
    {
        ConfigurarMedicoes(
            DadosDeTeste.NovaMedicao(_usuarioId, sintomas: Sintoma.FaltaDeAr | Sintoma.Tontura),
            DadosDeTeste.NovaMedicao(_usuarioId, sintomas: Sintoma.FaltaDeAr),
            DadosDeTeste.NovaMedicao(_usuarioId, sintomas: Sintoma.Nenhum));

        var resumo = await _servico.ObterResumoAsync(_usuarioId);

        resumo.Sintomas.FaltaDeAr.Should().Be(2);
        resumo.Sintomas.Tontura.Should().Be(1);
        resumo.Sintomas.DorNoPeito.Should().Be(0);
        resumo.Sintomas.ComAlgumSintoma.Should().Be(2);
        resumo.Sintomas.SemSintomas.Should().Be(1);
    }
}
