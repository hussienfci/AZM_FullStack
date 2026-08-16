using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    /// <summary>
    /// Get all movies with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MoviesListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoviesListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _movieService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get movie by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponseDto>> GetById(int id)
    {
        var response = await _movieService.GetByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Search movies with filters
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(MoviesListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoviesListResponseDto>> Search([FromBody] MovieSearchDto searchDto)
    {
        var response = await _movieService.SearchAsync(searchDto);
        return Ok(response);
    }

    /// <summary>
    /// Get movies by genre
    /// </summary>
    [HttpGet("genre/{genre}")]
    [ProducesResponseType(typeof(MoviesListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoviesListResponseDto>> GetByGenre(string genre, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _movieService.GetByGenreAsync(genre, page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get top rated movies
    /// </summary>
    [HttpGet("top-rated")]
    [ProducesResponseType(typeof(MoviesListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoviesListResponseDto>> GetTopRated([FromQuery] int count = 10)
    {
        var response = await _movieService.GetTopRatedAsync(count);
        return Ok(response);
    }

    /// <summary>
    /// Create a new movie
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponseDto>> Create([FromBody] CreateMovieDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new MovieResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _movieService.CreateAsync(dto);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    /// <summary>
    /// Update an existing movie
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponseDto>> Update(int id, [FromBody] UpdateMovieDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new MovieResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _movieService.UpdateAsync(id, dto);

        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Delete a movie
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponseDto>> Delete(int id)
    {
        var response = await _movieService.DeleteAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Toggle movie active status
    /// </summary>
    [HttpPatch("{id:int}/toggle-status")]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponseDto>> ToggleStatus(int id)
    {
        var response = await _movieService.ToggleActiveStatusAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}