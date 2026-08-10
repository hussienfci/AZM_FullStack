using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MoviePlatform.Modules.Catalog.Domain.Entities;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Commands.CreateMovie;

public class CreateMovieHandler : IRequestHandler<CreateMovieCommand, Result<Guid>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMovieHandler(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IUnitOfWork unitOfWork)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var exists = await _movieRepository.ExistsByTitleAsync(request.Title, cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"A movie with the title '{request.Title}' already exists.");

        var movie = Movie.Create(
            request.Title,
            request.Description,
            request.ReleaseYear,
            request.PosterUrl,
            request.BackdropUrl,
            request.TrailerUrl,
            request.DurationMinutes,
            request.Director,
            request.Language);

        var genres = await _genreRepository.GetByIdsAsync(request.GenreIds, cancellationToken);
        foreach (var genre in genres)
        {
            movie.AddGenre(genre);
        }

        await _movieRepository.AddAsync(movie, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movie.Id);
    }
}
