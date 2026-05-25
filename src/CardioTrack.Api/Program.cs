using System.Text.Json.Serialization;
using CardioTrack.Api.Comum;
using CardioTrack.Application;
using CardioTrack.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Camadas de aplicacao e infraestrutura: cada uma registra seus proprios
// servicos, mantendo a API como simples composicao das camadas.
builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
        // Enums (Sexo, sintomas) trafegam como texto, mais legivel para o front-end.
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);
builder.Services.AdicionarCors(builder.Configuration);
builder.Services.AdicionarSwagger();

var app = builder.Build();

// O tratamento de excecoes vem primeiro para capturar falhas de todo o pipeline.
app.UseMiddleware<MiddlewareDeTratamentoDeExcecoes>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(ConfiguracaoDeServicos.PoliticaCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposto para os testes de integracao (WebApplicationFactory<Program>).
public partial class Program;
