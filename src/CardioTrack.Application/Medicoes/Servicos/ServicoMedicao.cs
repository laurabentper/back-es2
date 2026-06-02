using CardioTrack.Application.Abstracoes.Persistencia;
using CardioTrack.Application.Comum;
using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Domain.Medicoes;
using FluentValidation;

namespace CardioTrack.Application.Medicoes.Servicos;

/// <summary>
/// Orquestra o registro, a atualizacao e a remocao de medicoes, garantindo que
/// pertencam a um usuario existente. As regras de faixa dos valores ficam na
/// entidade <see cref="Medicao"/>.
/// </summary>
public sealed class ServicoMedicao : IServicoMedicao
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioMedicao _repositorioMedicao;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;
    private readonly IValidator<RegistrarMedicaoRequisicao> _validador;
    private readonly IValidator<AtualizarMedicaoRequisicao> _validadorAtualizacao;

    public ServicoMedicao(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioMedicao repositorioMedicao,
        IUnidadeDeTrabalho unidadeDeTrabalho,
        IValidator<RegistrarMedicaoRequisicao> validador,
        IValidator<AtualizarMedicaoRequisicao> validadorAtualizacao)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioMedicao = repositorioMedicao;
        _unidadeDeTrabalho = unidadeDeTrabalho;
        _validador = validador;
        _validadorAtualizacao = validadorAtualizacao;
    }

    public async Task<MedicaoResposta> RegistrarAsync(
        Guid usuarioId,
        RegistrarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        await _validador.ValidarOuLancarAsync(requisicao, cancellationToken);

        var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new ExcecaoDeNaoEncontrado("Usuario nao encontrado.");

        var sintomas = SintomasResposta.ParaFlags(
            requisicao.FaltaDeAr,
            requisicao.DorNoPeito,
            requisicao.Tontura);

        var medicao = usuario.RegistrarMedicao(
            requisicao.PressaoSistolica,
            requisicao.PressaoDiastolica,
            requisicao.FrequenciaCardiaca,
            requisicao.OxigenacaoSangue,
            requisicao.PesoCorporal,
            sintomas,
            requisicao.RegistradaEm);

        await _repositorioMedicao.AdicionarAsync(medicao, cancellationToken);
        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return MedicaoResposta.DeDominio(medicao);
    }

    public async Task<MedicaoResposta> AtualizarAsync(
        Guid usuarioId,
        Guid medicaoId,
        AtualizarMedicaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        await _validadorAtualizacao.ValidarOuLancarAsync(requisicao, cancellationToken);

        var medicao = await ObterDoUsuarioAsync(usuarioId, medicaoId, cancellationToken);

        var sintomas = SintomasResposta.ParaFlags(
            requisicao.FaltaDeAr,
            requisicao.DorNoPeito,
            requisicao.Tontura);

        medicao.Atualizar(
            requisicao.PressaoSistolica,
            requisicao.PressaoDiastolica,
            requisicao.FrequenciaCardiaca,
            requisicao.OxigenacaoSangue,
            requisicao.PesoCorporal,
            sintomas,
            requisicao.RegistradaEm);

        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return MedicaoResposta.DeDominio(medicao);
    }

    public async Task RemoverAsync(
        Guid usuarioId,
        Guid medicaoId,
        CancellationToken cancellationToken = default)
    {
        var medicao = await ObterDoUsuarioAsync(usuarioId, medicaoId, cancellationToken);

        _repositorioMedicao.Remover(medicao);
        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtem a medicao garantindo que ela pertenca ao usuario autenticado. Trata um
    /// id inexistente e um id de outro usuario da mesma forma (404), evitando
    /// revelar a existencia de medicoes alheias.
    /// </summary>
    private async Task<Medicao> ObterDoUsuarioAsync(
        Guid usuarioId,
        Guid medicaoId,
        CancellationToken cancellationToken)
    {
        var medicao = await _repositorioMedicao.ObterPorIdAsync(medicaoId, cancellationToken);

        if (medicao is null || medicao.UsuarioId != usuarioId)
        {
            throw new ExcecaoDeNaoEncontrado("Medicao nao encontrada.");
        }

        return medicao;
    }
}
