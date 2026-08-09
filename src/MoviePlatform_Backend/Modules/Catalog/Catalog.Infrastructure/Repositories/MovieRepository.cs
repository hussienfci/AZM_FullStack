using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoviePlatform.Modules.Catalog.Domain.Entities;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Modules.Catalog.Infrastructure.Persistence;
using MoviePlatform.Shared.Kernel.Pagination;

namespace MoviePlatform.Modules.Catalog.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly CatalogDbContext _context;

    public MovieRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<Movie?> GetByIdWithGenresAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<Movie>> SearchAsync(
        string? searchQuery,
        int? releaseYear,
        Guid? genreId,
        bool? isFeatured,
        string? sortBy,
        bool descending,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchLower = searchQuery.ToLower();
            query = query.Where(m => 
                m.Title.ToLower().Contains(searchLower) ||
                m.Description.ToLower().Contains(searchLower) ||
                (m.Director != null && m.Director.ToLower().Contains(searchLower)));
        }

        // Year filter
        if (releaseYear.HasValue)
        {
            query = query.Where(m => m.ReleaseYear == releaseYear.Value);
        }

        // Genre filter
        if (genreId.HasValue)
        {
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        // Featured filter
        if (isFeatured.HasValue)
        {
            query = query.Where(m => m.IsFeatured == isFeatured.Value);
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => descending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "releaseyear" => descending ? query.OrderByDescending(m => m.ReleaseYear) : query.OrderBy(m => m.ReleaseYear),
            "rating" => descending ? query.OrderByDescending(m => m.AverageRating) : query.OrderBy(m => m.AverageRating),
            _ => descending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Movie>
        {
            Items = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<Movie>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Where(m => m.IsFeatured && (m.FeaturedUntil == null || m.FeaturedUntil > DateTime.UtcNow))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Movie>> GetTrendingAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .OrderByDescending(m => m.ReviewCount)
            .ThenByDescending(m => m.AverageRating)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AnyAsync(m => m.Title.ToLower() == title.ToLower(), cancellationToken);
    }

    public async Task<Movie> AddAsync(Movie entity, CancellationToken cancellationToken = default)
    {
        await _context.Movies.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Movie entity, CancellationToken cancellationToken = default)
    {
        _context.Movies.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await _context.Movies.FindAsync(new object[] { id }, cancellationToken);
        if (movie != null)
        {
            movie.MarkAsDeleted();
            _context.Movies.Update(movie);
        }
    }
}
