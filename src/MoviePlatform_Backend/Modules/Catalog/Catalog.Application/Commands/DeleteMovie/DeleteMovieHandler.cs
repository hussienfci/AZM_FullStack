using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Shared.Kernel.Interfaces;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Commands.DeleteMovie;

public class DeleteMovieHandler : IRequestHandler<DeleteMovieCommand, Result>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMovieHandler(IMovieRepository movieRepository, IUnitOfWork unitOfWork)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);
        if (movie == null)
            return Result.Failure($"Movie with ID {request.MovieId} not found.");

        await _movieRepository.DeleteAsync(request.MovieId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
