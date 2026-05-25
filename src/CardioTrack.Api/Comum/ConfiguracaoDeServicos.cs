using System.Text;
using CardioTrack.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CardioTrack.Api.Comum;

/// <summary>
/// Concentra a configuracao dos servicos transversais da API (autenticacao JWT,
/// CORS e Swagger), mantendo o <c>Program</c> enxuto e legivel.
/// </summary>
public static class ConfiguracaoDeServicos
{
    public const string PoliticaCors = "CardioTrackCors";

    /// <summary>
    /// Configura a autenticacao por JWT Bearer usando as mesmas opcoes que a
    /// infraestrutura emprega para gerar os tokens, garantindo que emissor,
    /// audiencia e chave de assinatura sejam validados de forma coerente.
    /// </summary>
    public static IServiceCollection AdicionarAutenticacaoJwt(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var opcoes = configuracao.GetSection(OpcoesJwt.Secao).Get<OpcoesJwt>()
            ?? throw new InvalidOperationException(
                "A secao de configuracao 'Jwt' nao foi encontrada.");

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcoes.SecretKey));

        servicos
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(parametros =>
            {
                // Mantem o nome original das claims (ex.: "sub") em vez do
                // mapeamento legado para URIs longas, simplificando a leitura.
                parametros.MapInboundClaims = false;
                parametros.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opcoes.Issuer,
                    ValidateAudience = true,
                    ValidAudience = opcoes.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = chave,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        servicos.AddAuthorization();
        return servicos;
    }

    /// <summary>
    /// Habilita o CORS para as origens listadas na secao "Cors:AllowedOrigins",
    /// permitindo que o front-end (em outra origem) consuma a API.
    /// </summary>
    public static IServiceCollection AdicionarCors(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var origens = configuracao.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        servicos.AddCors(opcoes => opcoes.AddPolicy(PoliticaCors, politica =>
        {
            if (origens.Length > 0)
            {
                politica.WithOrigins(origens);
            }

            politica.AllowAnyHeader().AllowAnyMethod();
        }));

        return servicos;
    }

    /// <summary>
    /// Configura o Swagger/OpenAPI com suporte a autenticacao JWT, exibindo o
    /// botao "Authorize" e exigindo o token Bearer nas rotas protegidas.
    /// </summary>
    public static IServiceCollection AdicionarSwagger(this IServiceCollection servicos)
    {
        servicos.AddEndpointsApiExplorer();
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CardioTrack API",
                Version = "v1",
                Description = "API de acompanhamento de saude cardiaca: contas de "
                    + "usuario, registro de medicoes e relatorios."
            });

            var esquema = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT obtido no login. Apenas o token, "
                    + "sem o prefixo 'Bearer'.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            opcoes.AddSecurityDefinition("Bearer", esquema);
            opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [esquema] = Array.Empty<string>()
            });

            opcoes.EnableAnnotations();
        });

        return servicos;
    }
}
