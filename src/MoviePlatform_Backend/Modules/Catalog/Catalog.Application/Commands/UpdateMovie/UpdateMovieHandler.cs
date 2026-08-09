using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Commands.UpdateMovie;

public class UpdateMovieHandler : IRequestHandler<UpdateMovieCommand, Result>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMovieHandler(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IUnitOfWork unitOfWork)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdWithGenresAsync(request.MovieId, cancellationToken);
        if (movie == null)
            return Result.Failure($"Movie with ID {request.MovieId} not found.");

        movie.UpdateDetails(
            request.Title,
            request.Description,
            request.ReleaseYear,
            request.PosterUrl,
            request.BackdropUrl,
            request.TrailerUrl,
            request.DurationMinutes,
            request.Director,
            request.Language);

        // Update genres
        var currentGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList();
        var genresToRemove = currentGenreIds.Except(request.GenreIds).ToList();
        var genresToAdd = request.GenreIds.Except(currentGenreIds).ToList();

        foreach (var genreId in genresToRemove)
        {
            movie.RemoveGenre(genreId);
        }

        var newGenres = await _genreRepository.GetByIdsAsync(genresToAdd, cancellationToken);
        foreach (var genre in newGenres)
        {
            movie.AddGenre(genre);
        }

        await _movieRepository.UpdateAsync(movie, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
