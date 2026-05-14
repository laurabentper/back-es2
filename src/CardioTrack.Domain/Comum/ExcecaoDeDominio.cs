namespace CardioTrack.Domain.Comum;

/// <summary>
/// Sinaliza a violacao de uma regra de negocio do dominio. A camada de API
/// traduz essa excecao em uma resposta de erro adequada ao cliente.
/// </summary>
public class ExcecaoDeDominio : Exception
{
    public ExcecaoDeDominio(string mensagem) : base(mensagem)
    {
    }
}
