using CardioTrack.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MySql;

namespace CardioTrack.Tests.Integration.Comum;

/// <summary>
/// Sobe a API em memoria apontada para um MySQL real, executado em um contêiner
/// efemero via Testcontainers. Assim os testes exercitam o mesmo provider de
/// banco usado em producao (Pomelo/MySQL), incluindo migrations, indices e
/// conversoes, sem depender de um banco instalado na maquina.
/// </summary>
public sealed class FabricaDeApi : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ChaveSecretaJwt =
        "chave-secreta-de-teste-com-no-minimo-32-bytes-de-tamanho";

    // Versao fixada do servidor para os testes: assim o DbContext nao precisa abrir
    // uma conexao ao montar as opcoes (como faz o AutoDetect da API), o que tornava
    // a inicializacao sensivel ao instante exato em que o MySQL fica pronto.
    private static readonly MySqlServerVersion VersaoServidor = new(new Version(8, 0, 0));

    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.0")
        .WithDatabase("cardiotrack")
        .WithUsername("cardiotrack")
        .WithPassword("cardiotrack")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Substitui o DbContext da API pelo do contêiner, com a versao do servidor
        // ja conhecida. No EF Core 9 a configuracao das opcoes vive em
        // IDbContextOptionsConfiguration; remove-la evita que o AutoDetect original
        // abra uma conexao ao resolver o contexto.
        builder.ConfigureTestServices(servicos =>
        {
            servicos.RemoveAll<IDbContextOptionsConfiguration<CardioTrackDbContext>>();
            servicos.RemoveAll<DbContextOptions<CardioTrackDbContext>>();
            servicos.RemoveAll<DbContextOptions>();

            servicos.AddDbContext<CardioTrackDbContext>(opcoes =>
                opcoes.UseMySql(_mysql.GetConnectionString(), VersaoServidor));
        });
    }

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        // As variaveis de ambiente sao lidas pela configuracao padrao do
        // WebApplication.CreateBuilder ja no momento em que a API registra os
        // servicos. Isso garante que a connection string e as opcoes de JWT estejam
        // visiveis tanto na geracao do token quanto na validacao (Bearer), evitando
        // divergencia de chave entre as duas pontas.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection", _mysql.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Issuer", "CardioTrack.Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "CardioTrack.Client");
        Environment.SetEnvironmentVariable("Jwt__SecretKey", ChaveSecretaJwt);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");

        // Acessar Services aqui forca a construcao do host (lendo as variaveis acima)
        // e permite aplicar as migrations no banco do contêiner.
        using var escopo = Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<CardioTrackDbContext>();
        await AplicarMigrationsComRetentativaAsync(contexto);
    }

    /// <summary>
    /// O MySQL pode reiniciar durante a primeira inicializacao do contêiner,
    /// recusando conexoes por alguns instantes; algumas retentativas tornam a
    /// aplicacao das migrations resiliente a essa janela.
    /// </summary>
    private static async Task AplicarMigrationsComRetentativaAsync(CardioTrackDbContext contexto)
    {
        const int tentativas = 10;
        for (var tentativa = 1; ; tentativa++)
        {
            try
            {
                await contexto.Database.MigrateAsync();
                return;
            }
            catch when (tentativa < tentativas)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mysql.DisposeAsync();
        await base.DisposeAsync();
    }
}
