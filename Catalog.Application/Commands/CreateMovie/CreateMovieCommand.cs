using System;
using System.Collections.Generic;
using MediatR;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Commands.CreateMovie;

public record CreateMovieCommand : IRequest<Result<Guid>>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public int ReleaseYear { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Director { get; set; }
    public string? Language { get; set; }
    public List<Guid> GenreIds { get; set; } = new();
}
