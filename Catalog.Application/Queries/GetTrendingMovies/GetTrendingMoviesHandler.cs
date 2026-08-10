using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetTrendingMovies;

public class GetTrendingMoviesHandler : IRequestHandler<GetTrendingMoviesQuery, Result<IReadOnlyList<MovieSummaryDto>>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetTrendingMoviesHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<MovieSummaryDto>>> Handle(GetTrendingMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetTrendingAsync(request.Count, cancellationToken);
        var movieDtos = _mapper.Map<IReadOnlyList<MovieSummaryDto>>(movies);
        return Result<IReadOnlyList<MovieSummaryDto>>.Success(movieDtos);
    }
}
