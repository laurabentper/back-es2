using CardioTrack.Infrastructure.Seguranca;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Infraestrutura.Seguranca;

public class ServicoDeHashDeSenhaTests
{
    private readonly ServicoDeHashDeSenha _servico = new();

    [Fact]
    public void GerarHash_NaoRetornaASenhaEmTextoPuro()
    {
        var hash = _servico.GerarHash("senhaForte123");

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe("senhaForte123");
    }

    [Fact]
    public void GerarHash_ParaMesmaSenha_ProduzHashesDistintos()
    {
        // O BCrypt incorpora um salt aleatorio, entao dois hashes da mesma senha diferem.
        var primeiro = _servico.GerarHash("senhaForte123");
        var segundo = _servico.GerarHash("senhaForte123");

        primeiro.Should().NotBe(segundo);
    }

    [Fact]
    public void Verificar_ComSenhaCorreta_RetornaVerdadeiro()
    {
        var hash = _servico.GerarHash("senhaForte123");

        _servico.Verificar("senhaForte123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verificar_ComSenhaIncorreta_RetornaFalso()
    {
        var hash = _servico.GerarHash("senhaForte123");

        _servico.Verificar("outraSenha", hash).Should().BeFalse();
    }
}
