using CardioTrack.Domain.Comum;
using CardioTrack.Domain.Usuarios;

namespace CardioTrack.Domain.Medicoes;

/// <summary>
/// Registro de uma medicao de saude cardiaca de um usuario: pressao arterial,
/// frequencia cardiaca, oxigenacao do sangue, peso corporal e sintomas.
/// </summary>
public class Medicao : Entidade
{
    // Construtor sem parametros exigido pelo EF Core para materializar a entidade.
    private Medicao()
    {
    }

    public Medicao(
        Guid usuarioId,
        int pressaoSistolica,
        int pressaoDiastolica,
        int frequenciaCardiaca,
        int oxigenacaoSangue,
        decimal pesoCorporal,
        Sintoma sintomas,
        DateTime? registradaEm = null)
    {
        Garantir.Que(usuarioId != Guid.Empty, "A medicao deve estar vinculada a um usuario.");
        Garantir.DentroDoIntervalo(pressaoSistolica, 50, 300, nameof(pressaoSistolica));
        Garantir.DentroDoIntervalo(pressaoDiastolica, 30, 200, nameof(pressaoDiastolica));
        Garantir.Que(pressaoSistolica > pressaoDiastolica,
            "A pressao sistolica deve ser maior que a diastolica.");
        Garantir.DentroDoIntervalo(frequenciaCardiaca, 20, 250, nameof(frequenciaCardiaca));
        Garantir.DentroDoIntervalo(oxigenacaoSangue, 50, 100, nameof(oxigenacaoSangue));
        Garantir.DentroDoIntervalo(pesoCorporal, 0.5m, 500m, nameof(pesoCorporal));

        var quando = registradaEm ?? DateTime.UtcNow;
        Garantir.Que(quando <= DateTime.UtcNow, "A data da medicao nao pode estar no futuro.");

        UsuarioId = usuarioId;
        PressaoSistolica = pressaoSistolica;
        PressaoDiastolica = pressaoDiastolica;
        FrequenciaCardiaca = frequenciaCardiaca;
        OxigenacaoSangue = oxigenacaoSangue;
        PesoCorporal = pesoCorporal;
        Sintomas = sintomas;
        RegistradaEm = quando;
        CriadaEm = DateTime.UtcNow;
    }

    /// <summary>Usuario dono da medicao.</summary>
    public Guid UsuarioId { get; private set; }

    public Usuario? Usuario { get; private set; }

    /// <summary>Pressao sistolica em mmHg.</summary>
    public int PressaoSistolica { get; private set; }

    /// <summary>Pressao diastolica em mmHg.</summary>
    public int PressaoDiastolica { get; private set; }

    /// <summary>Frequencia cardiaca em batimentos por minuto.</summary>
    public int FrequenciaCardiaca { get; private set; }

    /// <summary>Oxigenacao do sangue (SpO2) em porcentagem.</summary>
    public int OxigenacaoSangue { get; private set; }

    /// <summary>Peso corporal em quilogramas.</summary>
    public decimal PesoCorporal { get; private set; }

    public Sintoma Sintomas { get; private set; }

    /// <summary>Momento em que a medicao foi realizada.</summary>
    public DateTime RegistradaEm { get; private set; }

    /// <summary>Momento em que a medicao foi cadastrada no sistema.</summary>
    public DateTime CriadaEm { get; private set; }

    public bool PossuiSintomas => Sintomas != Sintoma.Nenhum;
}
