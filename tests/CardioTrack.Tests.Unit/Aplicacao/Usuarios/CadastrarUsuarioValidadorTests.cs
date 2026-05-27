using CardioTrack.Application.Usuarios.Validacoes;
using CardioTrack.Domain.Usuarios;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Aplicacao.Usuarios;

public class CadastrarUsuarioValidadorTests
{
    private readonly CadastrarUsuarioValidador _validador = new();

    [Fact]
    public void Validar_ComRequisicaoValida_NaoApontaErros()
    {
        var resultado = _validador.Validate(DadosDeTeste.CadastroValido());

        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    public void Validar_ComEmailInvalido_ApontaErroNoEmail(string email)
    {
        var resultado = _validador.Validate(DadosDeTeste.CadastroValido(email: email));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validar_ComSenhaCurta_ApontaErroNaSenha()
    {
        var resultado = _validador.Validate(DadosDeTeste.CadastroValido(senha: "1234567"));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Validar_ComConfirmacaoDiferente_ApontaErroNaConfirmacao()
    {
        var resultado = _validador.Validate(
            DadosDeTeste.CadastroValido(senha: "senhaForte123", confirmacaoSenha: "diferente"));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ConfirmacaoSenha");
    }

    [Fact]
    public void Validar_ComDataNascimentoNoFuturo_ApontaErro()
    {
        var futuro = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);

        var resultado = _validador.Validate(DadosDeTeste.CadastroValido(dataNascimento: futuro));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DataNascimento");
    }

    [Fact]
    public void Validar_ComSexoForaDoEnum_ApontaErro()
    {
        var resultado = _validador.Validate(DadosDeTeste.CadastroValido(sexo: (Sexo)99));

        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Sexo");
    }
}
