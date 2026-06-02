using CardioTrack.Application.Medicoes.Dtos;
using FluentValidation;

namespace CardioTrack.Application.Medicoes.Validacoes;

/// <summary>
/// Valida os dados de atualizacao de uma medicao antes do dominio, com mensagens
/// amigaveis. Os intervalos espelham as invariantes da entidade <c>Medicao</c>.
/// </summary>
public sealed class AtualizarMedicaoValidador : AbstractValidator<AtualizarMedicaoRequisicao>
{
    public AtualizarMedicaoValidador()
    {
        RuleFor(r => r.PressaoSistolica)
            .InclusiveBetween(50, 300)
            .WithMessage("A pressao sistolica deve estar entre 50 e 300 mmHg.");

        RuleFor(r => r.PressaoDiastolica)
            .InclusiveBetween(30, 200)
            .WithMessage("A pressao diastolica deve estar entre 30 e 200 mmHg.");

        RuleFor(r => r)
            .Must(r => r.PressaoSistolica > r.PressaoDiastolica)
            .WithName(nameof(AtualizarMedicaoRequisicao.PressaoSistolica))
            .WithMessage("A pressao sistolica deve ser maior que a diastolica.");

        RuleFor(r => r.FrequenciaCardiaca)
            .InclusiveBetween(20, 250)
            .WithMessage("A frequencia cardiaca deve estar entre 20 e 250 bpm.");

        RuleFor(r => r.OxigenacaoSangue)
            .InclusiveBetween(50, 100)
            .WithMessage("A oxigenacao do sangue deve estar entre 50 e 100%.");

        RuleFor(r => r.PesoCorporal)
            .InclusiveBetween(0.5m, 500m)
            .WithMessage("O peso corporal deve estar entre 0,5 e 500 kg.");

        RuleFor(r => r.RegistradaEm)
            .Must(data => data is null || data.Value <= DateTime.UtcNow)
            .WithMessage("A data da medicao nao pode estar no futuro.");
    }
}
