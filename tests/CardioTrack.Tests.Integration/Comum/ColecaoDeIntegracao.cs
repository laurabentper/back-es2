namespace CardioTrack.Tests.Integration.Comum;

/// <summary>
/// Compartilha uma unica <see cref="FabricaDeApi"/> (e, portanto, um unico
/// contêiner MySQL) entre todas as classes de teste de integracao, evitando o
/// custo de subir o banco repetidas vezes. Cada teste usa e-mails unicos para
/// permanecer isolado, mesmo com o banco compartilhado.
/// </summary>
[CollectionDefinition(Nome)]
public sealed class ColecaoDeIntegracao : ICollectionFixture<FabricaDeApi>
{
    public const string Nome = "Integracao";
}
