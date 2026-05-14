using CardioTrack.Domain.Comum;
using CardioTrack.Domain.Medicoes;

namespace CardioTrack.Domain.Usuarios;

/// <summary>
/// Usuario do sistema. Agrega seus dados de cadastro e o historico de medicoes
/// de saude cardiaca. A senha e mantida apenas como hash; o texto puro nunca
/// chega ao dominio.
/// </summary>
public class Usuario : Entidade
{
    private readonly List<Medicao> _medicoes = new();

    // Construtor sem parametros exigido pelo EF Core para materializar a entidade.
    private Usuario()
    {
    }

    public Usuario(
        string nome,
        string sobrenome,
        string email,
        string telefone,
        string senhaHash,
        DateOnly dataNascimento,
        Sexo sexo,
        string paisResidencia)
    {
        Garantir.NaoVazio(nome, nameof(nome));
        Garantir.NaoVazio(sobrenome, nameof(sobrenome));
        Garantir.NaoVazio(email, nameof(email));
        Garantir.Que(email.Contains('@'), "O e-mail informado e invalido.");
        Garantir.NaoVazio(telefone, nameof(telefone));
        Garantir.NaoVazio(senhaHash, nameof(senhaHash));
        Garantir.NaoVazio(paisResidencia, nameof(paisResidencia));
        Garantir.Que(dataNascimento < DateOnly.FromDateTime(DateTime.UtcNow),
            "A data de nascimento deve estar no passado.");

        Nome = nome.Trim();
        Sobrenome = sobrenome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone.Trim();
        SenhaHash = senhaHash;
        DataNascimento = dataNascimento;
        Sexo = sexo;
        PaisResidencia = paisResidencia.Trim();
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;

    public string Sobrenome { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Telefone { get; private set; } = string.Empty;

    /// <summary>Hash da senha. O texto puro nunca e armazenado.</summary>
    public string SenhaHash { get; private set; } = string.Empty;

    public DateOnly DataNascimento { get; private set; }

    public Sexo Sexo { get; private set; }

    public string PaisResidencia { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; }

    public IReadOnlyCollection<Medicao> Medicoes => _medicoes.AsReadOnly();

    public string NomeCompleto => $"{Nome} {Sobrenome}";

    /// <summary>
    /// Cria uma nova medicao vinculada a este usuario e a adiciona ao seu
    /// historico, garantindo que toda medicao pertenca a um usuario valido.
    /// </summary>
    public Medicao RegistrarMedicao(
        int pressaoSistolica,
        int pressaoDiastolica,
        int frequenciaCardiaca,
        int oxigenacaoSangue,
        decimal pesoCorporal,
        Sintoma sintomas,
        DateTime? registradaEm = null)
    {
        var medicao = new Medicao(
            Id,
            pressaoSistolica,
            pressaoDiastolica,
            frequenciaCardiaca,
            oxigenacaoSangue,
            pesoCorporal,
            sintomas,
            registradaEm);

        _medicoes.Add(medicao);
        return medicao;
    }
}
