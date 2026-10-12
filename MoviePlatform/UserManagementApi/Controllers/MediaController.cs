using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Services.Storage;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/media")]
[AllowAnonymous]
public class MediaController : ControllerBase
{
    private readonly IImageStorage _imageStorage;

    public MediaController(IImageStorage imageStorage) => _imageStorage = imageStorage;

    /// <summary>
    /// Serve a stored poster image.
    /// </summary>
    [HttpGet("posters/{fileName}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    public IActionResult GetPoster(string fileName)
    {
        var poster = _imageStorage.OpenPoster(fileName);
        if (poster is null) return NotFound();
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        return File(poster.Value.Content, poster.Value.ContentType);
    }
}
