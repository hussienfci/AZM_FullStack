using System;
using MediatR;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Commands.DeleteMovie;

public record DeleteMovieCommand(Guid MovieId) : IRequest<Result>;
