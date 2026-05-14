namespace CardioTrack.Domain.Medicoes;

/// <summary>
/// Sintomas associados a uma medicao. Modelado como flags para que uma medicao
/// possa registrar nenhum, um ou varios sintomas simultaneamente.
/// </summary>
[Flags]
public enum Sintoma
{
    Nenhum = 0,
    FaltaDeAr = 1,
    DorNoPeito = 2,
    Tontura = 4
}
