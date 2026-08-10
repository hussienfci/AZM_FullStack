using System;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Shared.Kernel.Pagination;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.SearchMovies;

public record SearchMoviesQuery : IRequest<Result<PagedList<MovieSummaryDto>>>
{
    public string? SearchQuery { get; set; }
    public int? ReleaseYear { get; set; }
    public Guid? GenreId { get; set; }
    public bool? IsFeatured { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
