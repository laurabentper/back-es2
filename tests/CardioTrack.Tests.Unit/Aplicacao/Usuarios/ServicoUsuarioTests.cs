using CardioTrack.Application.Abstracoes.Persistencia;
using CardioTrack.Application.Abstracoes.Seguranca;
using CardioTrack.Application.Comum;
using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Application.Usuarios.Servicos;
using CardioTrack.Application.Usuarios.Validacoes;
using CardioTrack.Domain.Usuarios;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;
using Moq;

namespace CardioTrack.Tests.Unit.Aplicacao.Usuarios;

public class ServicoUsuarioTests
{
    private readonly Mock<IRepositorioUsuario> _repositorio = new();
    private readonly Mock<IUnidadeDeTrabalho> _unidadeDeTrabalho = new();
    private readonly Mock<IServicoDeHashDeSenha> _hashDeSenha = new();
    private readonly Mock<IGeradorDeToken> _geradorDeToken = new();
    private readonly ServicoUsuario _servico;

    public ServicoUsuarioTests()
    {
        // Validadores reais: as regras ja sao cobertas em testes proprios e aqui
        // garantem que o servico realmente as aciona.
        _servico = new ServicoUsuario(
            _repositorio.Object,
            _unidadeDeTrabalho.Object,
            _hashDeSenha.Object,
            _geradorDeToken.Object,
            new CadastrarUsuarioValidador(),
            new AutenticarUsuarioValidador());
    }

    [Fact]
    public async Task CadastrarAsync_ComDadosValidos_PersisteRetornandoOUsuario()
    {
        var requisicao = DadosDeTeste.CadastroValido(email: "NOVA@Exemplo.com");
        _repositorio
            .Setup(r => r.EmailJaCadastradoAsync("NOVA@Exemplo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hashDeSenha.Setup(h => h.GerarHash(requisicao.Senha)).Returns("hash-gerado");

        var resposta = await _servico.CadastrarAsync(requisicao);

        resposta.Email.Should().Be("nova@exemplo.com");
        resposta.NomeCompleto.Should().Be($"{requisicao.Nome} {requisicao.Sobrenome}");
        _repositorio.Verify(
            r => r.AdicionarAsync(
                It.Is<Usuario>(u => u.Email == "nova@exemplo.com" && u.SenhaHash == "hash-gerado"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unidadeDeTrabalho.Verify(u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CadastrarAsync_QuandoEmailJaCadastrado_LancaConflitoENaoPersiste()
    {
        var requisicao = DadosDeTeste.CadastroValido();
        _repositorio
            .Setup(r => r.EmailJaCadastradoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var acao = async () => await _servico.CadastrarAsync(requisicao);

        await acao.Should().ThrowAsync<ExcecaoDeConflito>();
        _repositorio.Verify(
            r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unidadeDeTrabalho.Verify(
            u => u.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CadastrarAsync_ComConfirmacaoDeSenhaDiferente_LancaValidacao()
    {
        var requisicao = DadosDeTeste.CadastroValido(senha: "senhaForte123", confirmacaoSenha: "outra");

        var acao = async () => await _servico.CadastrarAsync(requisicao);

        await acao.Should().ThrowAsync<ExcecaoDeValidacao>();
        _repositorio.Verify(
            r => r.EmailJaCadastradoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AutenticarAsync_ComCredenciaisCorretas_RetornaTokenEUsuario()
    {
        var usuario = DadosDeTeste.NovoUsuario(email: "maria@exemplo.com", senhaHash: "hash");
        var requisicao = DadosDeTeste.LoginValido(email: "maria@exemplo.com", senha: "senhaForte123");
        var expiraEm = DateTime.UtcNow.AddHours(8);

        _repositorio
            .Setup(r => r.ObterPorEmailAsync("maria@exemplo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _hashDeSenha.Setup(h => h.Verificar("senhaForte123", "hash")).Returns(true);
        _geradorDeToken.Setup(g => g.Gerar(usuario)).Returns(new TokenGerado("token-jwt", expiraEm));

        var resposta = await _servico.AutenticarAsync(requisicao);

        resposta.Token.Should().Be("token-jwt");
        resposta.ExpiraEm.Should().Be(expiraEm);
        resposta.Usuario.Email.Should().Be("maria@exemplo.com");
    }

    [Fact]
    public async Task AutenticarAsync_QuandoUsuarioNaoExiste_LancaCredenciaisInvalidas()
    {
        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var acao = async () => await _servico.AutenticarAsync(DadosDeTeste.LoginValido());

        await acao.Should().ThrowAsync<ExcecaoDeCredenciaisInvalidas>();
        _geradorDeToken.Verify(g => g.Gerar(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AutenticarAsync_QuandoSenhaIncorreta_LancaCredenciaisInvalidas()
    {
        var usuario = DadosDeTeste.NovoUsuario(senhaHash: "hash");
        _repositorio
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _hashDeSenha.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var acao = async () => await _servico.AutenticarAsync(DadosDeTeste.LoginValido());

        await acao.Should().ThrowAsync<ExcecaoDeCredenciaisInvalidas>();
        _geradorDeToken.Verify(g => g.Gerar(It.IsAny<Usuario>()), Times.Never);
    }
}
