using CardioTrack.Domain.Medicoes;

namespace CardioTrack.Application.Abstracoes.Persistencia;

/// <summary>
/// Contrato de acesso a persistencia de medicoes. Alem do cadastro, expoe
/// consultas usadas pelos relatorios de historico e dados agregados.
/// </summary>
public interface IRepositorioMedicao
{
    Task AdicionarAsync(Medicao medicao, CancellationToken cancellationToken = default);

    /// <summary>Remove uma medicao ja persistida.</summary>
    void Remover(Medicao medicao);

    Task<Medicao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as medicoes de um usuario, opcionalmente filtradas por periodo,
    /// ordenadas da mais recente para a mais antiga.
    /// </summary>
    Task<IReadOnlyList<Medicao>> ListarPorUsuarioAsync(
        Guid usuarioId,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default);
}
