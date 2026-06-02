using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Domain.Medicoes;
using CardioTrack.Domain.Usuarios;

namespace CardioTrack.Tests.Unit.Comum;

/// <summary>
/// Fabrica de objetos validos para os testes, centralizando os valores padrao e
/// expondo apenas o que cada cenario precisa sobrescrever. Mantem os testes
/// focados na regra exercitada, sem repetir a montagem completa das entidades.
/// </summary>
public static class DadosDeTeste
{
    public static Usuario NovoUsuario(
        string nome = "Maria",
        string sobrenome = "Silva",
        string email = "maria@exemplo.com",
        string telefone = "11999998888",
        string senhaHash = "hash-da-senha",
        DateOnly? dataNascimento = null,
        Sexo sexo = Sexo.Feminino,
        string paisResidencia = "Brasil") =>
        new(
            nome,
            sobrenome,
            email,
            telefone,
            senhaHash,
            dataNascimento ?? new DateOnly(1990, 5, 20),
            sexo,
            paisResidencia);

    public static Medicao NovaMedicao(
        Guid? usuarioId = null,
        int pressaoSistolica = 120,
        int pressaoDiastolica = 80,
        int frequenciaCardiaca = 70,
        int oxigenacaoSangue = 98,
        decimal pesoCorporal = 72.5m,
        Sintoma sintomas = Sintoma.Nenhum,
        DateTime? registradaEm = null) =>
        new(
            usuarioId ?? Guid.NewGuid(),
            pressaoSistolica,
            pressaoDiastolica,
            frequenciaCardiaca,
            oxigenacaoSangue,
            pesoCorporal,
            sintomas,
            registradaEm);

    public static CadastrarUsuarioRequisicao CadastroValido(
        string nome = "Maria",
        string sobrenome = "Silva",
        string email = "maria@exemplo.com",
        string telefone = "11999998888",
        string senha = "senhaForte123",
        string? confirmacaoSenha = null,
        DateOnly? dataNascimento = null,
        Sexo sexo = Sexo.Feminino,
        string paisResidencia = "Brasil") =>
        new(
            nome,
            sobrenome,
            email,
            telefone,
            senha,
            confirmacaoSenha ?? senha,
            dataNascimento ?? new DateOnly(1990, 5, 20),
            sexo,
            paisResidencia);

    public static AutenticarUsuarioRequisicao LoginValido(
        string email = "maria@exemplo.com",
        string senha = "senhaForte123") =>
        new(email, senha);

    public static RegistrarMedicaoRequisicao MedicaoValida(
        int pressaoSistolica = 120,
        int pressaoDiastolica = 80,
        int frequenciaCardiaca = 70,
        int oxigenacaoSangue = 98,
        decimal pesoCorporal = 72.5m,
        bool faltaDeAr = false,
        bool dorNoPeito = false,
        bool tontura = false,
        DateTime? registradaEm = null) =>
        new(
            pressaoSistolica,
            pressaoDiastolica,
            frequenciaCardiaca,
            oxigenacaoSangue,
            pesoCorporal,
            faltaDeAr,
            dorNoPeito,
            tontura,
            registradaEm);

    public static AtualizarMedicaoRequisicao AtualizacaoMedicaoValida(
        int pressaoSistolica = 130,
        int pressaoDiastolica = 85,
        int frequenciaCardiaca = 75,
        int oxigenacaoSangue = 97,
        decimal pesoCorporal = 71.0m,
        bool faltaDeAr = false,
        bool dorNoPeito = false,
        bool tontura = false,
        DateTime? registradaEm = null) =>
        new(
            pressaoSistolica,
            pressaoDiastolica,
            frequenciaCardiaca,
            oxigenacaoSangue,
            pesoCorporal,
            faltaDeAr,
            dorNoPeito,
            tontura,
            registradaEm);
}
