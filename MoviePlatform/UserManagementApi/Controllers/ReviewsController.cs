using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reviews with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReviewsListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewsListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _reviewService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get review by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> GetById(int id)
    {
        var response = await _reviewService.GetByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Get reviews by movie ID
    /// </summary>
    [HttpGet("movie/{movieId:int}")]
    [ProducesResponseType(typeof(MovieReviewsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MovieReviewsResponseDto>> GetByMovie(int movieId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _reviewService.GetByMovieIdAsync(movieId, page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Get reviews by user ID
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(ReviewsListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewsListResponseDto>> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _reviewService.GetByUserIdAsync(userId, page, pageSize);
        return Ok(response);
    }

    /// <summary>
    /// Create a new review
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReviewResponseDto>> Create([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ReviewResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _reviewService.CreateAsync(dto);

        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    /// <summary>
    /// Update an existing review
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReviewResponseDto>> Update(int id, [FromBody] UpdateReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ReviewResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _reviewService.UpdateAsync(id, dto);

        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Delete a review
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> Delete(int id)
    {
        var response = await _reviewService.DeleteAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Toggle review active status
    /// </summary>
    [HttpPatch("{id:int}/toggle-status")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> ToggleStatus(int id)
    {
        var response = await _reviewService.ToggleActiveStatusAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}