using System.Text.Json;
using CardioTrack.Application.Comum;
using Microsoft.AspNetCore.Mvc;

namespace CardioTrack.Api.Comum;

/// <summary>
/// Captura as excecoes lancadas pelos casos de uso e as traduz em respostas HTTP
/// padronizadas (<see cref="ProblemDetails"/>), mantendo os controllers e os
/// servicos livres de tratamento de erro repetitivo. Cada tipo de
/// <see cref="ExcecaoDeAplicacao"/> mapeia um codigo de status especifico; falhas
/// inesperadas viram um 500 generico, sem vazar detalhes internos.
/// </summary>
public sealed class MiddlewareDeTratamentoDeExcecoes
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<MiddlewareDeTratamentoDeExcecoes> _logger;

    public MiddlewareDeTratamentoDeExcecoes(
        RequestDelegate proximo,
        ILogger<MiddlewareDeTratamentoDeExcecoes> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (Exception excecao)
        {
            await TratarAsync(contexto, excecao);
        }
    }

    private async Task TratarAsync(HttpContext contexto, Exception excecao)
    {
        var problema = CriarProblema(excecao);

        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(excecao, "Erro nao tratado ao processar a requisicao.");
        }

        contexto.Response.StatusCode = problema.Status!.Value;
        contexto.Response.ContentType = "application/problem+json";

        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        await contexto.Response.WriteAsync(JsonSerializer.Serialize(problema, opcoes));
    }

    private static ProblemDetails CriarProblema(Exception excecao) => excecao switch
    {
        ExcecaoDeValidacao validacao => CriarProblemaDeValidacao(validacao),
        ExcecaoDeCredenciaisInvalidas => Problema(
            StatusCodes.Status401Unauthorized, "Credenciais invalidas", excecao.Message),
        ExcecaoDeNaoEncontrado => Problema(
            StatusCodes.Status404NotFound, "Recurso nao encontrado", excecao.Message),
        ExcecaoDeConflito => Problema(
            StatusCodes.Status409Conflict, "Conflito", excecao.Message),
        ExcecaoDeAplicacao => Problema(
            StatusCodes.Status400BadRequest, "Requisicao invalida", excecao.Message),
        _ => Problema(
            StatusCodes.Status500InternalServerError,
            "Erro interno",
            "Ocorreu um erro inesperado ao processar a requisicao.")
    };

    private static ProblemDetails CriarProblemaDeValidacao(ExcecaoDeValidacao excecao)
    {
        var problema = new ValidationProblemDetails(excecao.Erros.ToDictionary(
            par => par.Key,
            par => par.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos sao invalidos."
        };

        return problema;
    }

    private static ProblemDetails Problema(int status, string titulo, string detalhe) => new()
    {
        Status = status,
        Title = titulo,
        Detail = detalhe
    };
}
