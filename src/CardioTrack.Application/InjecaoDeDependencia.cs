using CardioTrack.Application.Medicoes.Dtos;
using CardioTrack.Application.Medicoes.Servicos;
using CardioTrack.Application.Medicoes.Validacoes;
using CardioTrack.Application.Relatorios.Servicos;
using CardioTrack.Application.Usuarios.Dtos;
using CardioTrack.Application.Usuarios.Servicos;
using CardioTrack.Application.Usuarios.Validacoes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CardioTrack.Application;

/// <summary>
/// Registra os servicos da camada de aplicacao (casos de uso e validadores) no
/// container de injecao de dependencia. A camada de API apenas chama este metodo.
/// </summary>
public static class InjecaoDeDependencia
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection servicos)
    {
        servicos.AddScoped<IServicoUsuario, ServicoUsuario>();
        servicos.AddScoped<IServicoMedicao, ServicoMedicao>();
        servicos.AddScoped<IServicoRelatorio, ServicoRelatorio>();

        servicos.AddScoped<IValidator<CadastrarUsuarioRequisicao>, CadastrarUsuarioValidador>();
        servicos.AddScoped<IValidator<AutenticarUsuarioRequisicao>, AutenticarUsuarioValidador>();
        servicos.AddScoped<IValidator<RegistrarMedicaoRequisicao>, RegistrarMedicaoValidador>();
        servicos.AddScoped<IValidator<AtualizarMedicaoRequisicao>, AtualizarMedicaoValidador>();

        return servicos;
    }
}
