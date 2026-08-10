using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Modules.Catalog.Domain.Entities;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Modules.Catalog.Infrastructure.Persistence;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly CatalogDbContext _context;

    public GenreRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Genre?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Genres
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Genres
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Genres
            .AsNoTracking()
            .Where(g => idList.Contains(g.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Genre> AddAsync(Genre entity, CancellationToken cancellationToken = default)
    {
        await _context.Genres.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Genre entity, CancellationToken cancellationToken = default)
    {
        _context.Genres.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var genre = await _context.Genres.FindAsync(new object[] { id }, cancellationToken);
        if (genre != null)
        {
            genre.MarkAsDeleted();
            _context.Genres.Update(genre);
        }
    }
}
