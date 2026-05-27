using CardioTrack.Application.Abstracoes.Persistencia;
using CardioTrack.Application.Comum;
using CardioTrack.Application.Medicoes.Servicos;
using CardioTrack.Application.Medicoes.Validacoes;
using CardioTrack.Domain.Medicoes;
using CardioTrack.Domain.Usuarios;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;
using Moq;

namespace CardioTrack.Tests.Unit.Aplicacao.Medicoes;

public class ServicoMedicaoTests
{
    private readonly Mock<IRepositorioUsuario> _repositorioUsuario = new();
    private readonly Mock<IRepositorioMedicao> _repositorioMedicao = new();
    private readonly Mock<IUnidadeDeTrabalho> _unidadeDeTrabalho = new();
    private readonly ServicoMedicao _servico;

    public ServicoMedicaoTests()
    {
        _servico = new ServicoMedicao(
            _repositorioUsuario.Object,
            _repositorioMedicao.Object,
            _unidadeDeTrabalho.Object,
            new RegistrarMedicaoValidador());
    }

    [Fact]
    public async Task RegistrarAsync_ComUsuarioExistente_PersisteMedicaoComSintomas()
    {
        var usuario = DadosDeTeste.NovoUsuario();
        _repositorioUsuario
            .Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        var requisicao = DadosDeTeste.MedicaoValida(faltaDeAr: true, tontura: true);

        var resposta = await _servico.RegistrarAsync(usuario.Id, requisicao);

        resposta.UsuarioId.Should().Be(usuario.Id);
        resposta.PossuiSintomas.Should().BeTrue();
        resposta.Sintomas.FaltaDeAr.Should().BeTrue();
        resposta.Sintomas.Tontura.Should().BeTrue();
        resposta.Sintomas.DorNoPeito.Should().BeFalse();
        _repositorioMedicao.Verify(
            r => r.AdicionarAsync(
                It.Is<Medicao>(m => m.UsuarioId == usuario.Id && m.Sintomas == (Sintoma.FaltaDeAr | Sintoma.Tontura)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_QuandoUsuarioNaoExiste_LancaNaoEncontrado()
    {
        _repositorioUsuario
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var acao = async () => await _servico.RegistrarAsync(Guid.NewGuid(), DadosDeTeste.MedicaoValida());

        await acao.Should().ThrowAsync<ExcecaoDeNaoEncontrado>();
        _repositorioMedicao.Verify(
            r => r.AdicionarAsync(It.IsAny<Medicao>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_ComDadosInvalidos_LancaValidacaoSemConsultarUsuario()
    {
        var requisicao = DadosDeTeste.MedicaoValida(pressaoSistolica: 80, pressaoDiastolica: 90);

        var acao = async () => await _servico.RegistrarAsync(Guid.NewGuid(), requisicao);

        await acao.Should().ThrowAsync<ExcecaoDeValidacao>();
        _repositorioUsuario.Verify(
            r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
