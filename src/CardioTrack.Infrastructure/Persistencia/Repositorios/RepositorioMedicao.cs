using CardioTrack.Application.Abstracoes.Persistencia;
using CardioTrack.Domain.Medicoes;
using Microsoft.EntityFrameworkCore;

namespace CardioTrack.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Repositorio de medicoes sobre o EF Core, incluindo a consulta por usuario e
/// periodo usada pelos relatorios.
/// </summary>
public class RepositorioMedicao : IRepositorioMedicao
{
    private readonly CardioTrackDbContext _contexto;

    public RepositorioMedicao(CardioTrackDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(Medicao medicao, CancellationToken cancellationToken = default) =>
        await _contexto.Medicoes.AddAsync(medicao, cancellationToken);

    public void Remover(Medicao medicao) =>
        _contexto.Medicoes.Remove(medicao);

    public Task<Medicao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _contexto.Medicoes.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Medicao>> ListarPorUsuarioAsync(
        Guid usuarioId,
        DateTime? inicio = null,
        DateTime? fim = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = _contexto.Medicoes
            .AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId);

        if (inicio is not null)
        {
            consulta = consulta.Where(m => m.RegistradaEm >= inicio.Value);
        }

        if (fim is not null)
        {
            consulta = consulta.Where(m => m.RegistradaEm <= fim.Value);
        }

        return await consulta
            .OrderByDescending(m => m.RegistradaEm)
            .ToListAsync(cancellationToken);
    }
}
