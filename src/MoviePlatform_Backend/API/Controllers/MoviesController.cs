using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MoviePlatform.Modules.Catalog.Application.Commands.CreateMovie;
using MoviePlatform.Modules.Catalog.Application.Commands.DeleteMovie;
using MoviePlatform.Modules.Catalog.Application.Commands.UpdateMovie;
using MoviePlatform.Modules.Catalog.Application.DTOs;
using MoviePlatform.Modules.Catalog.Application.Queries.GetAllGenres;
using MoviePlatform.Modules.Catalog.Application.Queries.GetFeaturedMovies;
using MoviePlatform.Modules.Catalog.Application.Queries.GetMovieDetails;
using MoviePlatform.Modules.Catalog.Application.Queries.GetTrendingMovies;
using MoviePlatform.Modules.Catalog.Application.Queries.SearchMovies;
using MoviePlatform.Shared.Kernel.Pagination;
using MoviePlatform.Shared.Kernel.Results;

namespace MoviePlatform.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedList<MovieSummaryDto>>>> SearchMovies(
        [FromQuery] SearchMoviesRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchMoviesQuery
        {
            SearchQuery = request.SearchQuery,
            ReleaseYear = request.ReleaseYear,
            GenreId = request.GenreId,
            IsFeatured = request.IsFeatured,
            SortBy = request.SortBy,
            Descending = request.Descending,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse.Success(result.Value));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MovieSummaryDto>>>> GetFeaturedMovies(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeaturedMoviesQuery(), cancellationToken);
        return Ok(ApiResponse.Success(result.Value));
    }

    [HttpGet("trending")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MovieSummaryDto>>>> GetTrendingMovies(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTrendingMoviesQuery(count), cancellationToken);
        return Ok(ApiResponse.Success(result.Value));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MovieDetailsDto>>> GetMovieDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMovieDetailsQuery(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(ApiResponse.Failure(result.Errors));

        return Ok(ApiResponse.Success(result.Value));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateMovie(
        [FromBody] CreateMovieCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Failure(result.Errors));

        return CreatedAtAction(
            nameof(GetMovieDetails),
            new { id = result.Value },
            ApiResponse.Success(result.Value));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdateMovie(
        Guid id,
        [FromBody] UpdateMovieCommand command,
        CancellationToken cancellationToken)
    {
        command.MovieId = id;
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Failure(result.Errors));

        return Ok(ApiResponse.Success());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteMovie(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteMovieCommand(id), cancellationToken);

        if (result.IsFailure)
            return NotFound(ApiResponse.Failure(result.Errors));

        return Ok(ApiResponse.Success());
    }

    [HttpGet("genres")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GenreDto>>>> GetAllGenres(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllGenresQuery(), cancellationToken);
        return Ok(ApiResponse.Success(result.Value));
    }
}

public class SearchMoviesRequest
{
    public string? SearchQuery { get; set; }
    public int? ReleaseYear { get; set; }
    public Guid? GenreId { get; set; }
    public bool? IsFeatured { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
