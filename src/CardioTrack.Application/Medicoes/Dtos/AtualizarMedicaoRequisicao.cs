namespace CardioTrack.Application.Medicoes.Dtos;

/// <summary>
/// Dados para atualizar uma medicao existente. Substitui integralmente os valores
/// da medicao; segue a mesma estrutura do cadastro, com os sintomas como booleanos
/// independentes combinados na flag <c>Sintoma</c> do dominio pelo servico.
/// </summary>
public sealed record AtualizarMedicaoRequisicao(
    int PressaoSistolica,
    int PressaoDiastolica,
    int FrequenciaCardiaca,
    int OxigenacaoSangue,
    decimal PesoCorporal,
    bool FaltaDeAr,
    bool DorNoPeito,
    bool Tontura,
    DateTime? RegistradaEm = null);
