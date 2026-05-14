namespace CardioTrack.Domain.Comum;

/// <summary>
/// Base de todas as entidades do dominio. Define a identidade e a comparacao
/// por identificador, independente dos demais atributos.
/// </summary>
public abstract class Entidade
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj) =>
        obj is Entidade outra && outra.GetType() == GetType() && outra.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}
