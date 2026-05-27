using CardioTrack.Domain.Comum;
using CardioTrack.Domain.Medicoes;
using CardioTrack.Domain.Usuarios;
using CardioTrack.Tests.Unit.Comum;
using FluentAssertions;

namespace CardioTrack.Tests.Unit.Dominio.Usuarios;

public class UsuarioTests
{
    [Fact]
    public void Construtor_ComDadosValidos_PreencheAsPropriedades()
    {
        var nascimento = new DateOnly(1985, 3, 10);

        var usuario = DadosDeTeste.NovoUsuario(
            nome: "Joao",
            sobrenome: "Souza",
            email: "joao@exemplo.com",
            telefone: "11988887777",
            senhaHash: "hash",
            dataNascimento: nascimento,
            sexo: Sexo.Masculino,
            paisResidencia: "Brasil");

        usuario.Nome.Should().Be("Joao");
        usuario.Sobrenome.Should().Be("Souza");
        usuario.Email.Should().Be("joao@exemplo.com");
        usuario.Telefone.Should().Be("11988887777");
        usuario.SenhaHash.Should().Be("hash");
        usuario.DataNascimento.Should().Be(nascimento);
        usuario.Sexo.Should().Be(Sexo.Masculino);
        usuario.PaisResidencia.Should().Be("Brasil");
        usuario.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        usuario.Medicoes.Should().BeEmpty();
    }

    [Fact]
    public void Construtor_NormalizaCamposComEspacosEEmailEmMinusculas()
    {
        var usuario = DadosDeTeste.NovoUsuario(
            nome: "  Maria  ",
            sobrenome: "  Silva ",
            email: "  MARIA@Exemplo.COM ",
            telefone: " 11999998888 ",
            paisResidencia: "  Brasil ");

        usuario.Nome.Should().Be("Maria");
        usuario.Sobrenome.Should().Be("Silva");
        usuario.Email.Should().Be("maria@exemplo.com");
        usuario.Telefone.Should().Be("11999998888");
        usuario.PaisResidencia.Should().Be("Brasil");
    }

    [Fact]
    public void NomeCompleto_ConcatenaNomeESobrenome()
    {
        var usuario = DadosDeTeste.NovoUsuario(nome: "Maria", sobrenome: "Silva");

        usuario.NomeCompleto.Should().Be("Maria Silva");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_ComNomeVazio_Lanca(string nome)
    {
        var acao = () => DadosDeTeste.NovoUsuario(nome: nome);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*nome*");
    }

    [Fact]
    public void Construtor_ComEmailSemArroba_Lanca()
    {
        var acao = () => DadosDeTeste.NovoUsuario(email: "sem-arroba.com");

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*e-mail*");
    }

    [Fact]
    public void Construtor_ComSenhaHashVazia_Lanca()
    {
        var acao = () => DadosDeTeste.NovoUsuario(senhaHash: " ");

        acao.Should().Throw<ExcecaoDeDominio>();
    }

    [Fact]
    public void Construtor_ComDataNascimentoNoFuturo_Lanca()
    {
        var futuro = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var acao = () => DadosDeTeste.NovoUsuario(dataNascimento: futuro);

        acao.Should().Throw<ExcecaoDeDominio>().WithMessage("*data de nascimento*");
    }

    [Fact]
    public void RegistrarMedicao_AdicionaAoHistoricoEVinculaAoUsuario()
    {
        var usuario = DadosDeTeste.NovoUsuario();

        var medicao = usuario.RegistrarMedicao(
            pressaoSistolica: 120,
            pressaoDiastolica: 80,
            frequenciaCardiaca: 70,
            oxigenacaoSangue: 98,
            pesoCorporal: 70m,
            sintomas: Sintoma.FaltaDeAr);

        usuario.Medicoes.Should().ContainSingle().Which.Should().BeSameAs(medicao);
        medicao.UsuarioId.Should().Be(usuario.Id);
        medicao.Sintomas.Should().Be(Sintoma.FaltaDeAr);
    }

    [Fact]
    public void RegistrarMedicao_ComValorInvalido_NaoAlteraHistorico()
    {
        var usuario = DadosDeTeste.NovoUsuario();

        var acao = () => usuario.RegistrarMedicao(
            pressaoSistolica: 10,
            pressaoDiastolica: 80,
            frequenciaCardiaca: 70,
            oxigenacaoSangue: 98,
            pesoCorporal: 70m,
            sintomas: Sintoma.Nenhum);

        acao.Should().Throw<ExcecaoDeDominio>();
        usuario.Medicoes.Should().BeEmpty();
    }
}
