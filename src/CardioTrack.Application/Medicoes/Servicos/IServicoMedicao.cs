using CardioTrack.Application.Medicoes.Dtos;

namespace CardioTrack.Application.Medicoes.Servicos;

/// <summary>
/// Casos de uso de registro, atualizacao e remocao de medicoes de saude cardiaca.
/// </summary>
public interface IServicoMedicao
{
    /// <summary>
    /// Registra uma medicao para o usuario informado. Lanca
    /// <c>ExcecaoDeValidacao</c> para dados invalidos e
    /// <c>ExcecaoDeNaoEncontrado</c> quando o usuario nao existe.
    /// </summary>
    Task<MedicaoResposta> RegistrarAsync(
        Guid usuarioId,
        RegistrarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma medicao do usuario informado. Lanca <c>ExcecaoDeValidacao</c>
    /// para dados invalidos e <c>ExcecaoDeNaoEncontrado</c> quando a medicao nao
    /// existe ou nao pertence ao usuario.
    /// </summary>
    Task<MedicaoResposta> AtualizarAsync(
        Guid usuarioId,
        Guid medicaoId,
        AtualizarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma medicao do usuario informado. Lanca <c>ExcecaoDeNaoEncontrado</c>
    /// quando a medicao nao existe ou nao pertence ao usuario.
    /// </summary>
    Task RemoverAsync(
        Guid usuarioId,
        Guid medicaoId,
        CancellationToken cancellationToken = default);
}
