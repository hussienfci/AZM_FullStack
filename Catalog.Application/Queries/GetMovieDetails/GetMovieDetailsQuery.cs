using System;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetMovieDetails;

public record GetMovieDetailsQuery(Guid MovieId) : IRequest<Result<MovieDetailsDto>>;
