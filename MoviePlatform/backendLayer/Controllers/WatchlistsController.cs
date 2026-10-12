using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WatchlistsController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<WatchlistsController> _logger;

    public WatchlistsController(IWatchlistService watchlistService, ILogger<WatchlistsController> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<WatchlistListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _watchlistService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WatchlistResponseDto>> GetById(int id)
    {
        var response = await _watchlistService.GetByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<UserWatchlistResponseDto>> GetByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _watchlistService.GetByUserIdAsync(userId, page, pageSize);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<WatchlistResponseDto>> Create([FromBody] CreateWatchlistDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new WatchlistResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _watchlistService.CreateAsync(dto);
        if (!response.Success) return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WatchlistResponseDto>> Update(int id, [FromBody] UpdateWatchlistDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new WatchlistResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _watchlistService.UpdateAsync(id, dto);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    [HttpPatch("{id:int}/mark-watched")]
    public async Task<ActionResult<WatchlistResponseDto>> MarkAsWatched(int id)
    {
        var response = await _watchlistService.MarkAsWatchedAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<WatchlistResponseDto>> Delete(int id)
    {
        var response = await _watchlistService.DeleteAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPatch("{id:int}/toggle-status")]
    public async Task<ActionResult<WatchlistResponseDto>> ToggleStatus(int id)
    {
        var response = await _watchlistService.ToggleActiveStatusAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
