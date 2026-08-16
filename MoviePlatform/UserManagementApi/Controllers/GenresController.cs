using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services.Interfaces;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;
    private readonly ILogger<GenresController> _logger;

    public GenresController(IGenreService genreService, ILogger<GenresController> logger)
    {
        _genreService = genreService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GenresListResponseDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _genreService.GetAllAsync(page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenreResponseDto>> GetById(int id)
    {
        var response = await _genreService.GetByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<GenreResponseDto>> GetByName(string name)
    {
        var response = await _genreService.GetByNameAsync(name);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<GenreResponseDto>> Create([FromBody] CreateGenreDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new GenreResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _genreService.CreateAsync(dto);
        if (!response.Success) return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GenreResponseDto>> Update(int id, [FromBody] UpdateGenreDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new GenreResponseDto { Success = false, Message = "Invalid model state" });

        var response = await _genreService.UpdateAsync(id, dto);
        if (!response.Success)
            return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<GenreResponseDto>> Delete(int id)
    {
        var response = await _genreService.DeleteAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPatch("{id:int}/toggle-status")]
    public async Task<ActionResult<GenreResponseDto>> ToggleStatus(int id)
    {
        var response = await _genreService.ToggleActiveStatusAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
