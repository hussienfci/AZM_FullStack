using System.Collections.Generic;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetFeaturedMovies;

public record GetFeaturedMoviesQuery : IRequest<Result<IReadOnlyList<MovieSummaryDto>>>;
