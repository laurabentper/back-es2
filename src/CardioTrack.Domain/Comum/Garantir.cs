namespace CardioTrack.Domain.Comum;

/// <summary>
/// Guardas reutilizaveis para validar invariantes do dominio. Quando uma
/// condicao nao e atendida, lanca <see cref="ExcecaoDeDominio"/>.
/// </summary>
public static class Garantir
{
    public static void Que(bool condicao, string mensagem)
    {
        if (!condicao)
        {
            throw new ExcecaoDeDominio(mensagem);
        }
    }

    public static void NaoVazio(string? valor, string campo) =>
        Que(!string.IsNullOrWhiteSpace(valor), $"O campo '{campo}' e obrigatorio.");

    public static void DentroDoIntervalo(int valor, int minimo, int maximo, string campo) =>
        Que(valor >= minimo && valor <= maximo,
            $"O campo '{campo}' deve estar entre {minimo} e {maximo}.");

    public static void DentroDoIntervalo(decimal valor, decimal minimo, decimal maximo, string campo) =>
        Que(valor >= minimo && valor <= maximo,
            $"O campo '{campo}' deve estar entre {minimo} e {maximo}.");
}
