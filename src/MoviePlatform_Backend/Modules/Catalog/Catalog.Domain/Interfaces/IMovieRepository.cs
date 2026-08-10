using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MoviePlatform.Modules.Catalog.Domain.Entities;
using MoviePlatform.Shared.Kernel.Interfaces;
using MoviePlatform.Shared.Kernel.Pagination;

namespace MoviePlatform.Modules.Catalog.Domain.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetByIdWithGenresAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedList<Movie>> SearchAsync(
        string? searchQuery,
        int? releaseYear,
        Guid? genreId,
        bool? isFeatured,
        string? sortBy,
        bool descending,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Movie>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Movie>> GetTrendingAsync(int count, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default);
}
