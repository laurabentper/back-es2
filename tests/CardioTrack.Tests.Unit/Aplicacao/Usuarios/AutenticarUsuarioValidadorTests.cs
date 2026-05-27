using CardioTrack.Application.Usuarios.Validacoes;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Aplicacao.Usuarios;

public class AutenticarUsuarioValidadorTests
{
    private readonly AutenticarUsuarioValidador _validador = new();

    [Fact]
    public void Validar_ComCredenciaisValidas_NaoApontaErros()
    {
        var resultado = _validador.Validate(DadosDeTeste.LoginValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalido")]
    public void Validar_ComEmailInvalido_ApontaErro(string email)
    {
        var resultado = _validador.Validate(DadosDeTeste.LoginValido(email: email));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validar_ComSenhaVazia_ApontaErro()
    {
        var resultado = _validador.Validate(DadosDeTeste.LoginValido(senha: ""));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }
}
