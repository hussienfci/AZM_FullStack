using System.Collections.Generic;
using MediatR;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.Modules.Catalog.Application.Queries.GetTrendingMovies;

public record GetTrendingMoviesQuery(int Count = 10) : IRequest<Result<IReadOnlyList<MovieSummaryDto>>>;
