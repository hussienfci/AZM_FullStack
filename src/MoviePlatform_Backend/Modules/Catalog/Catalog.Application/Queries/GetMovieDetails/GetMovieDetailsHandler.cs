using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetMovieDetails;

public class GetMovieDetailsHandler : IRequestHandler<GetMovieDetailsQuery, Result<MovieDetailsDto>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public GetMovieDetailsHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<Result<MovieDetailsDto>> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdWithGenresAsync(request.MovieId, cancellationToken);

        if (movie == null)
            return Result<MovieDetailsDto>.Failure($"Movie with ID {request.MovieId} not found.");

        var movieDto = _mapper.Map<MovieDetailsDto>(movie);
        return Result<MovieDetailsDto>.Success(movieDto);
    }
}
