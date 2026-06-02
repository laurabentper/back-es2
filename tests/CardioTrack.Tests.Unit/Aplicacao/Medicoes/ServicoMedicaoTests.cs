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
            new RegistrarMedicaoValidador(),
            new AtualizarMedicaoValidador());
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

    [Fact]
    public async Task AtualizarAsync_ComMedicaoDoUsuario_AlteraValoresEPersiste()
    {
        var usuarioId = Guid.NewGuid();
        var medicao = DadosDeTeste.NovaMedicao(usuarioId);
        _repositorioMedicao
            .Setup(r => r.ObterPorIdAsync(medicao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicao);
        var requisicao = DadosDeTeste.AtualizacaoMedicaoValida(pressaoSistolica: 140, dorNoPeito: true);

        var resposta = await _servico.AtualizarAsync(usuarioId, medicao.Id, requisicao);

        resposta.PressaoSistolica.Should().Be(140);
        resposta.Sintomas.DorNoPeito.Should().BeTrue();
        medicao.PressaoSistolica.Should().Be(140);
        medicao.Sintomas.Should().Be(Sintoma.DorNoPeito);
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoMedicaoNaoExiste_LancaNaoEncontrado()
    {
        _repositorioMedicao
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Medicao?)null);

        var acao = async () => await _servico.AtualizarAsync(
            Guid.NewGuid(), Guid.NewGuid(), DadosDeTeste.AtualizacaoMedicaoValida());

        await acao.Should().ThrowAsync<ExcecaoDeNaoEncontrado>();
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoMedicaoEDeOutroUsuario_LancaNaoEncontrado()
    {
        var medicao = DadosDeTeste.NovaMedicao(Guid.NewGuid());
        _repositorioMedicao
            .Setup(r => r.ObterPorIdAsync(medicao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicao);

        var acao = async () => await _servico.AtualizarAsync(
            Guid.NewGuid(), medicao.Id, DadosDeTeste.AtualizacaoMedicaoValida());

        await acao.Should().ThrowAsync<ExcecaoDeNaoEncontrado>();
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverAsync_ComMedicaoDoUsuario_RemoveEPersiste()
    {
        var usuarioId = Guid.NewGuid();
        var medicao = DadosDeTeste.NovaMedicao(usuarioId);
        _repositorioMedicao
            .Setup(r => r.ObterPorIdAsync(medicao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicao);

        await _servico.RemoverAsync(usuarioId, medicao.Id);

        _repositorioMedicao.Verify(r => r.Remover(medicao), Times.Once);
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_QuandoMedicaoEDeOutroUsuario_LancaNaoEncontrado()
    {
        var medicao = DadosDeTeste.NovaMedicao(Guid.NewGuid());
        _repositorioMedicao
            .Setup(r => r.ObterPorIdAsync(medicao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(medicao);

        var acao = async () => await _servico.RemoverAsync(Guid.NewGuid(), medicao.Id);

        await acao.Should().ThrowAsync<ExcecaoDeNaoEncontrado>();
        _repositorioMedicao.Verify(r => r.Remover(It.IsAny<Medicao>()), Times.Never);
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
