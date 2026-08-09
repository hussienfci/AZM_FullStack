using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetFeaturedMovies;

public class GetFeaturedMoviesHandler : IRequestHandler<GetFeaturedMoviesQuery, Result<IReadOnlyList<MovieSummaryDto>>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetFeaturedMoviesHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<MovieSummaryDto>>> Handle(GetFeaturedMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetFeaturedAsync(cancellationToken);
        var movieDtos = _mapper.Map<IReadOnlyList<MovieSummaryDto>>(movies);
        return Result<IReadOnlyList<MovieSummaryDto>>.Success(movieDtos);
    }
}
