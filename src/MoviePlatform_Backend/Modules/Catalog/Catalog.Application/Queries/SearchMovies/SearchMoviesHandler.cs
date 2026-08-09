using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Pagination;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.SearchMovies;

public class SearchMoviesHandler : IRequestHandler<SearchMoviesQuery, Result<PagedList<MovieSummaryDto>>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public SearchMoviesHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<MovieSummaryDto>>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
    {
        var pagination = new PaginationParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var movies = await _movieRepository.SearchAsync(
            request.SearchQuery,
            request.ReleaseYear,
            request.GenreId,
            request.IsFeatured,
            request.SortBy,
            request.Descending,
            pagination,
            cancellationToken);

        var movieDtos = _mapper.Map<List<MovieSummaryDto>>(movies.Items);

        var result = new PagedList<MovieSummaryDto>
        {
            Items = movieDtos,
            PageNumber = movies.PageNumber,
            PageSize = movies.PageSize,
            TotalCount = movies.TotalCount
        };

        return Result<PagedList<MovieSummaryDto>>.Success(result);
    }
}
