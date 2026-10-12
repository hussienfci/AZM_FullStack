<!-- squad-kit: story-id=poster-upload-api, feature=movie-image-attachment, title="Poster upload, replace and remove API", planner=claude-opus-5-5, generated=2026-10-12 -->

# Plan: Poster upload, replace and remove API

## 1. Overview

Add the ability to attach one uploaded poster image to an existing movie. Images are stored on disk
behind an `IImageStorage` abstraction (swappable for object storage later), served anonymously from a
stable URL, and that URL is written to the existing `Movie.PosterUrl` column, so it flows through the
unchanged `MovieDto`.

**Feature:** movie-image-attachment
**Story:** poster-upload-api
**Scope:** Backend only (`MoviePlatform/backendLayer`) plus a new test project.
**Depends on:** nothing new. Uses the existing JWT auth (`ClaimTypes.Role` is already mapped in `Program.cs`).

All paths are relative to `MoviePlatform/` unless they say otherwise.

---

## 2. Context & Current State

### 2.1 Stack
- .NET 10 (`net10.0`, SDK 10.0.401), ASP.NET Core controllers, EF Core 10 on SQL Server (LocalDB), AutoMapper 16.
- Central package management: `backendLayer/Directory.Packages.props`. **Any new package for the API project needs a `<PackageVersion>` row there.** This story adds **no** packages to the API project.
- `Program.cs` auto-migrates on start-up and seeds dev data (`Data/DatabaseSeeder.cs`, currently uncommitted; leave it alone).
- The dev API listens on `http://localhost:5247`. The Angular dev server proxies only `/api/*` to it (`Frontend/frontend layer/proxy.conf.json`). **This is why the poster URL must start with `/api/`.**

### 2.2 Relevant existing code
- `Models/Movie.cs`: `[StringLength(500)] public string? PosterUrl`. **There is no schema change and no migration.**
- `Models/DTOs/MovieDto.cs`: `MovieDto`, `CreateMovieDto`, `UpdateMovieDto` all carry `string? PosterUrl`. `MovieResponseDto { bool Success; string Message; MovieDto? Data; }`.
- `Services/MovieService.cs`: every method wraps work in `try/catch`, logs with `_logger`, and returns `MovieResponseDto { Success = false, Message = ... }` on failure. `"Movie not found"` is the not-found message (the controller checks `Message.Contains("not found")`).
- `Controllers/MoviesController.cs`: `[ApiController]`, `[Route("api/[controller]")]`, **no `[Authorize]` anywhere**. Every movie write endpoint is currently anonymous.
- `Controllers/UsersController.cs` uses `[Authorize(Roles = "Admin")]`. `UserRole` is `User | Admin | Moderator`.
- `Mappings/MappingProfile.cs`: `UpdateMovieDto → Movie` maps every field, including `PosterUrl`.

### 2.3 Decisions made in this plan (the feature left these open)
| Question | Decision |
|---|---|
| Who may upload/remove? | `[Authorize(Roles = "Admin")]` on the two new write endpoints only. Hardening the existing anonymous create/update/delete endpoints is a **separate follow-up** (see §8), so existing callers keep working. |
| Storage provider | `LocalImageStorage` writing under `{ContentRootPath}/App_Data/uploads/posters` (configurable). **Not** `wwwroot`. Files are only reachable through the media endpoint. |
| Stable URL format | `/api/media/posters/{32-hex-guid}.{jpg\|png\|webp}`. This is what gets stored in `Movie.PosterUrl`. It is relative, so it works through the Angular proxy and has no machine-specific path. |
| How do we know a file is "ours"? | A `PosterUrl` is *managed* only if it matches exactly `^/api/media/posters/[0-9a-f]{32}\.(jpg\|png\|webp)$`. Anything else, such as external `https://...` URLs, is never deleted (AC 5). |
| Formats / size | JPEG, PNG, WebP. **Max 5 MB (5 242 880 bytes).** The type is detected from **magic bytes**. The client's `Content-Type` and file name are ignored for the decision and never used to build the stored name. |
| Fetch auth | `GET /api/media/posters/{file}` is `[AllowAnonymous]`, because `<img src>` cannot send the JWT. Posters are public catalogue art. |

---

## 3. API contract (new)

```
PUT    /api/movies/{id:int}/poster   multipart/form-data, field "file"   [Admin]
         200 MovieResponseDto  (Data.PosterUrl = "/api/media/posters/<guid>.<ext>")
         400 MovieResponseDto  missing file / empty / too large / unsupported format
         401 / 403             not signed in / not Admin
         404 MovieResponseDto  movie not found
         413                   body exceeds the request size limit (Kestrel)

DELETE /api/movies/{id:int}/poster                                   [Admin]
         200 MovieResponseDto  (Data.PosterUrl = null). Idempotent: also 200 when there is no poster.
         404 MovieResponseDto  movie not found

GET    /api/media/posters/{fileName}                                 [AllowAnonymous]
         200 image/jpeg | image/png | image/webp, Cache-Control: public, max-age=31536000, immutable
         404                   unknown or invalid file name
```
Error messages (use these exact strings so the tests and frontend can rely on them):
- `"No file was uploaded"`
- `"File is too large. Maximum size is 5 MB"`
- `"Unsupported image format. Allowed: JPEG, PNG, WebP"`
- `"Movie not found"` (existing)

---

## 4. Implementation Steps (In Order)

### Step 1: Options + configuration
**New file:** `backendLayer/Services/Storage/ImageStorageOptions.cs`
```csharp
namespace UserManagementApi.Services.Storage;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>Root folder for stored images. Relative paths resolve against ContentRootPath.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    public long MaxPosterBytes { get; set; } = 5 * 1024 * 1024;
}
```
**Modify:** `backendLayer/appsettings.json`. Add a top-level section:
```json
"ImageStorage": { "RootPath": "App_Data/uploads", "MaxPosterBytes": 5242880 }
```
**Modify:** the repo-root `.gitignore`. Append the line `MoviePlatform/backendLayer/App_Data/` (outside the squad-kit managed block).

### Step 2: Image format detection (pure, unit-testable)
**New file:** `backendLayer/Services/Storage/ImageFormat.cs`
```csharp
namespace UserManagementApi.Services.Storage;

public enum ImageKind { Jpeg, Png, WebP }

public static class ImageFormat
{
    /// <summary>Detects the image type from the first bytes of the stream. The stream position is restored.</summary>
    public static ImageKind? Detect(Stream stream) { ... }

    public static string Extension(ImageKind kind) => kind switch { ImageKind.Jpeg => "jpg", ImageKind.Png => "png", _ => "webp" };
    public static string ContentType(ImageKind kind) => kind switch { ImageKind.Jpeg => "image/jpeg", ImageKind.Png => "image/png", _ => "image/webp" };
}
```
`Detect` reads up to 12 bytes into a buffer, then resets `stream.Position` to its original value. The stream must be seekable; callers pass a `MemoryStream`. Signatures:
- JPEG: `FF D8 FF`
- PNG: `89 50 4E 47 0D 0A 1A 0A`
- WebP: bytes 0–3 `"RIFF"` **and** bytes 8–11 `"WEBP"` (ASCII)

Return `null` for anything else or for fewer bytes than needed.

### Step 3: Storage abstraction + local implementation
**New file:** `backendLayer/Services/Storage/IImageStorage.cs`
```csharp
namespace UserManagementApi.Services.Storage;

public interface IImageStorage
{
    /// <summary>Stores the image and returns its stable public URL (e.g. /api/media/posters/abc.jpg).</summary>
    Task<string> SavePosterAsync(Stream content, ImageKind kind, CancellationToken ct = default);

    /// <summary>True only for URLs this storage created. External or unknown URLs return false.</summary>
    bool IsManagedPosterUrl(string? url);

    /// <summary>Deletes a managed poster. Does nothing (returns false) for non-managed URLs or missing files.</summary>
    Task<bool> DeletePosterAsync(string? url, CancellationToken ct = default);

    /// <summary>Opens a managed poster by file name for serving, or null if the name is invalid or missing.</summary>
    (Stream Content, string ContentType)? OpenPoster(string fileName);
}
```
**New file:** `backendLayer/Services/Storage/LocalImageStorage.cs`
- Constructor: `(IOptions<ImageStorageOptions> options, IWebHostEnvironment env, ILogger<LocalImageStorage> logger)`. Resolve `_postersDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.RootPath, "posters"))`, then `Directory.CreateDirectory(_postersDir)`. When `RootPath` is absolute, `Path.Combine` already returns it unchanged.
- `public const string PosterUrlPrefix = "/api/media/posters/";`
- `private static readonly Regex FileNamePattern = new(@"^[0-9a-f]{32}\.(jpg|png|webp)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);`
- `SavePosterAsync`: `var name = $"{Guid.NewGuid():N}.{ImageFormat.Extension(kind)}";` Write it with `new FileStream(path, FileMode.CreateNew, FileAccess.Write)` and `content.CopyToAsync`. Return `PosterUrlPrefix + name`.
- `IsManagedPosterUrl(url)`: `url` is non-null, starts with `PosterUrlPrefix` (ordinal), and the remainder matches `FileNamePattern`.
- `DeletePosterAsync`: return `false` if not managed. Otherwise build the path from the file name only and `File.Delete` it when it exists. Catch `IOException` / `UnauthorizedAccessException`, log a warning, and return `false`. **Never throw**, because callers use this as best-effort cleanup.
- `OpenPoster(fileName)`: return `null` unless `FileNamePattern` matches. Build the path, check it exists, and return `(File.OpenRead(path), ImageFormat.ContentType(kindFromExtension))`.
- Defence in depth: after `Path.Combine(_postersDir, fileName)`, also assert `Path.GetFullPath(p).StartsWith(_postersDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)`. The regex already blocks traversal, but this check stays in place.

### Step 4: Register services and the request limit
**Modify:** `backendLayer/Program.cs` (it has uncommitted edits; add to it, don't rewrite it)
- `using UserManagementApi.Services.Storage;`
- Under `// ── Services ──`:
  ```csharp
  builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));
  builder.Services.AddSingleton<IImageStorage, LocalImageStorage>();
  ```
No middleware changes. Do **not** add `UseStaticFiles` for the uploads folder.

### Step 5: Service methods
**Modify:** `backendLayer/Services/Interfaces/IMovieService.cs`. Add:
```csharp
Task<MovieResponseDto> SetPosterAsync(int id, IFormFile? file, CancellationToken ct = default);
Task<MovieResponseDto> RemovePosterAsync(int id, CancellationToken ct = default);
```
(`IFormFile` needs `using Microsoft.AspNetCore.Http;`.)

**Modify:** `backendLayer/Services/MovieService.cs`
- Inject `IImageStorage _imageStorage` and `IOptions<ImageStorageOptions> options` (store `_maxPosterBytes = options.Value.MaxPosterBytes`).
- `SetPosterAsync`. **The order matters for AC 3: nothing changes unless everything is valid.**
  1. `file is null || file.Length == 0` → `{ Success=false, Message="No file was uploaded" }`
  2. `file.Length > _maxPosterBytes` → `"File is too large. Maximum size is 5 MB"`
  3. `var movie = await _context.Movies.FindAsync([id], ct);` null → `"Movie not found"`
  4. Copy to `await using var buffer = new MemoryStream();`, then `await file.CopyToAsync(buffer, ct); buffer.Position = 0;`. If `ImageFormat.Detect(buffer)` is null → `"Unsupported image format. Allowed: JPEG, PNG, WebP"`.
  5. `var oldUrl = movie.PosterUrl; var newUrl = await _imageStorage.SavePosterAsync(buffer, kind, ct);`
  6. `movie.PosterUrl = newUrl; movie.UpdatedAt = DateTime.UtcNow;` In `try { await _context.SaveChangesAsync(ct); } catch { await _imageStorage.DeletePosterAsync(newUrl); throw; }`. This prevents orphan files when the DB write fails.
  7. **After** a successful save: `if (oldUrl != newUrl) await _imageStorage.DeletePosterAsync(oldUrl);` This is a no-op for external URLs (AC 5).
  8. Log `"Poster set for movie {MovieId}"` and return `{ Success=true, Message="Poster uploaded successfully", Data=_mapper.Map<MovieDto>(movie) }`.
  - The outer `try/catch` follows the existing style: log the error and return `"An error occurred while uploading poster"`.
- `RemovePosterAsync`:
  1. Find the movie; if it is missing → `"Movie not found"`.
  2. `var oldUrl = movie.PosterUrl;` If `oldUrl` is null or whitespace, return success with the current DTO (idempotent).
  3. `movie.PosterUrl = null; movie.UpdatedAt = UtcNow; await SaveChangesAsync(ct);`, then `await _imageStorage.DeletePosterAsync(oldUrl);`.
  4. Message `"Poster removed successfully"`.
- **Compatibility for existing create/update (AC 6). Keep accepting `PosterUrl` in the JSON:**
  - Add `private bool IsAcceptablePosterUrl(string? url, string? currentUrl)`. It returns true when `url` is null or whitespace, **or** `url == currentUrl`, **or** `Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == "http" || u.Scheme == "https")`.
  - In `CreateAsync` (current = `null`) and `UpdateAsync` (current = the loaded `movie.PosterUrl`, checked **before** `_mapper.Map`): if the URL is not acceptable, return `{ Success=false, Message="PosterUrl must be an absolute http(s) URL; use PUT /api/movies/{id}/poster to upload an image" }`. This stops clients from pointing a movie at someone else's managed file, while still allowing a full-object PUT that echoes back the movie's own managed URL.
  - In `UpdateAsync`, capture `oldUrl` before mapping. After `SaveChangesAsync`, `if (oldUrl != movie.PosterUrl) await _imageStorage.DeletePosterAsync(oldUrl);`
  - In `DeleteAsync`, capture `movie.PosterUrl` and call `DeletePosterAsync` after `SaveChangesAsync`.
  - Normalise whitespace-only `PosterUrl` to `null` in Create and Update.

### Step 6: Controller endpoints
**Modify:** `backendLayer/Controllers/MoviesController.cs`. Add `using Microsoft.AspNetCore.Authorization;` and append:
```csharp
private const long PosterRequestLimit = 6 * 1024 * 1024; // 5 MB file + multipart overhead

/// <summary>
/// Upload or replace the movie's poster (JPEG, PNG or WebP, max 5 MB). Admin only.
/// </summary>
[HttpPut("{id:int}/poster")]
[Authorize(Roles = "Admin")]
[Consumes("multipart/form-data")]
[RequestSizeLimit(PosterRequestLimit)]
[RequestFormLimits(MultipartBodyLengthLimit = PosterRequestLimit)]
[ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status404NotFound)]
public async Task<ActionResult<MovieResponseDto>> UploadPoster(int id, IFormFile? file, CancellationToken ct)
{
    var response = await _movieService.SetPosterAsync(id, file, ct);
    if (!response.Success)
        return response.Message.Contains("not found") ? NotFound(response) : BadRequest(response);
    return Ok(response);
}

/// <summary>
/// Remove the movie's poster. Admin only.
/// </summary>
[HttpDelete("{id:int}/poster")]
[Authorize(Roles = "Admin")]
... RemovePosterAsync → NotFound / Ok
```
Notes:
- The parameter name `file` must match the multipart field name. With `[ApiController]`, an `IFormFile` parameter binds from the form automatically, so don't add `[FromForm]` (Swashbuckle handles it either way).
- `[ApiController]` returns an automatic 400 `ProblemDetails` when model binding fails. That is acceptable. A missing `file` binds as `null` because it is nullable, so it reaches the service's "No file was uploaded" message.
- The service also catches a generic failure with the message `"An error occurred while uploading poster"`, which ends in `BadRequest`. That matches how the existing `Update` behaves.

**New file:** `backendLayer/Controllers/MediaController.cs`
```csharp
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
```
File names are random GUIDs and never rewritten, so long-lived caching is safe. A replaced poster gets a new URL.

### Step 7: Tests (new project)
There is no test project yet. Create one **next to** the API project, not inside it. Otherwise the API's default compile glob picks it up.

From `MoviePlatform/`:
```bash
dotnet new xunit -o UserManagementApi.Tests -f net10.0
dotnet add UserManagementApi.Tests reference backendLayer/UserManagementApi.csproj
dotnet add UserManagementApi.Tests package Microsoft.EntityFrameworkCore.InMemory --version 10.0.12
dotnet add UserManagementApi.Tests package Microsoft.AspNetCore.Mvc.Testing --version 10.0.12   # only if FrameworkReference is insufficient; see below
```
- `backendLayer/Directory.Packages.props` lives inside the API folder, so it does **not** apply to the test project. Normal `Version=` attributes are fine there.
- Add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to the test csproj so `IFormFile` / `FormFile` are available. Drop the Mvc.Testing package if it isn't needed.
- If `dotnet build` complains about NuGet restore with central management, add `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>` to the test csproj.

**`UserManagementApi.Tests/ImageFormatTests.cs`**
1. JPEG / PNG / WebP magic bytes → the correct `ImageKind`.
2. A GIF header (`47 49 46 38`), a text file, and a 2-byte stream → `null`.
3. `"RIFF....WAVE"` → `null` (RIFF but not WebP).
4. The stream position is unchanged after `Detect`.

**`UserManagementApi.Tests/LocalImageStorageTests.cs`**: use a temp dir (`Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))`) as an absolute `RootPath`, and delete it in `Dispose`. Fake `IWebHostEnvironment` with a tiny stub class.
1. `SavePosterAsync` returns a URL matching `^/api/media/posters/[0-9a-f]{32}\.png$` and the file exists.
2. `IsManagedPosterUrl`: true for a saved URL; false for `https://example.com/a.jpg`, `/api/media/posters/../x.jpg`, `/api/media/posters/ABC.jpg`, `null`, and `""`.
3. `DeletePosterAsync` removes a managed file; returns false and touches nothing for an external URL; returns false for a missing managed file.
4. `OpenPoster("../appsettings.json")` and `OpenPoster("x.gif")` → `null`.

**`UserManagementApi.Tests/MoviePosterServiceTests.cs`**: `AppDbContext` with `UseInMemoryDatabase(Guid.NewGuid().ToString())`, a real `MapperConfiguration` built from `MappingProfile`, `NullLogger<MovieService>`, and a real `LocalImageStorage` on a temp dir. Helper `FormFile Png()` builds a `FormFile` over a `MemoryStream` containing the PNG signature plus a few padding bytes.
1. **upload attaches the poster**: the DTO `PosterUrl` is managed and the DB row was updated.
2. **unknown movie → "Movie not found"**: no file is written to the posters dir.
3. **invalid format** (GIF bytes) → error; the existing `PosterUrl` is unchanged and no new file exists.
4. **too large** (options with `MaxPosterBytes = 10`) → error; nothing changes.
5. **null / empty file** → `"No file was uploaded"`.
6. **replace deletes the old managed file**: upload twice; the first file is gone and the second exists.
7. **replace keeps an external URL's "file"**: seed `PosterUrl = "https://example.com/p.jpg"`, upload, and assert success. Nothing is deleted outside the temp dir (assert via a storage spy, or just that no exception occurs and the new URL is set).
8. **remove clears the URL and deletes the file**; **remove twice** → both succeed.
9. **UpdateAsync with an external https URL still works** (AC 6); **UpdateAsync with someone else's managed URL** is rejected; **UpdateAsync echoing its own managed URL** succeeds and keeps the file.
10. **DeleteAsync of a movie** deletes its managed poster file.

If `AppDbContext`'s model uses SQL-Server-only configuration that InMemory rejects, report the error instead of changing `AppDbContext`.

### Step 8: Build & test
From `MoviePlatform/`:
```bash
dotnet build backendLayer/UserManagementApi.csproj
dotnet test UserManagementApi.Tests
```
Both must pass with zero errors. Do **not** run `dotnet ef migrations add`, because the schema does not change.

---

## 5. Testing Checklist

### 5.1 Automated
- [ ] `dotnet build`: no errors, no new warnings in the touched files.
- [ ] `dotnet test UserManagementApi.Tests`: all of the tests above pass.

### 5.2 Manual (Swagger at `http://localhost:5247/swagger`)
You need an Admin JWT. Self-registration creates `User`, so promote your account first:
`UPDATE Users SET Role = 'Admin' WHERE Email = '<you>'` in LocalDB `UserManagementDb`, then log in again to get a fresh token.
1. `PUT /api/movies/1/poster` with a PNG → 200, and `data.posterUrl` is `/api/media/posters/<guid>.png`.
2. Open `http://localhost:5247/api/media/posters/<guid>.png` in the browser with no token → the image is shown.
3. `GET /api/movies/1` → the same `posterUrl`.
4. Upload a JPEG to the same movie → new URL; the old file is gone from `backendLayer/App_Data/uploads/posters`.
5. Upload a `.txt` renamed to `.png` → 400 "Unsupported image format…"; `posterUrl` is unchanged.
6. Upload a file larger than 6 MB → 413; a 5–6 MB file → 400 "File is too large…".
7. `PUT /api/movies/99999/poster` → 404.
8. Without a token → 401; with a `User`-role token → 403.
9. `DELETE /api/movies/1/poster` → 200 with `posterUrl: null`; the file is gone; calling it again → 200.
10. `PUT /api/movies/2` (JSON) with `posterUrl: "https://example.com/p.jpg"` → 200 (compatibility).

---

## 6. Files Summary

### New files
| File | Purpose |
|------|---------|
| `backendLayer/Services/Storage/ImageStorageOptions.cs` | Root path and max size config |
| `backendLayer/Services/Storage/ImageFormat.cs` | Magic-byte detection, extension and content type |
| `backendLayer/Services/Storage/IImageStorage.cs` | Storage abstraction |
| `backendLayer/Services/Storage/LocalImageStorage.cs` | Disk implementation; managed-URL ownership rules |
| `backendLayer/Controllers/MediaController.cs` | Anonymous `GET /api/media/posters/{file}` |
| `UserManagementApi.Tests/*` | xUnit project and the three test classes |

### Modified files
| File | Changes |
|------|---------|
| `backendLayer/Program.cs` | Register options and `IImageStorage` |
| `backendLayer/appsettings.json` | `ImageStorage` section |
| `backendLayer/Services/Interfaces/IMovieService.cs` | `SetPosterAsync`, `RemovePosterAsync` |
| `backendLayer/Services/MovieService.cs` | New methods; PosterUrl validation and managed-file cleanup in Create/Update/Delete |
| `backendLayer/Controllers/MoviesController.cs` | `PUT` / `DELETE {id}/poster` (Admin) |
| `.gitignore` (repo root) | Ignore `App_Data/` |

### Not touched
`Models/Movie.cs`, `MovieDto.cs`, `MappingProfile.cs`, `AppDbContext.cs`, migrations, `DatabaseSeeder.cs`, and the frontend.

---

## 7. Rollback Plan
1. Revert the modified files (`git checkout -- <file>`), then delete the new files and `UserManagementApi.Tests/`.
2. Optional data cleanup: `UPDATE Movies SET PosterUrl = NULL WHERE PosterUrl LIKE '/api/media/posters/%'`, then delete `backendLayer/App_Data/uploads/`.

There is no schema change to revert.

---

## 8. Out of Scope / Follow-ups
- **Protect the existing movie write endpoints** (`POST`, `PUT`, `DELETE`, `PATCH toggle-status` are anonymous today). Recommended as the next story. It is not part of this one, so existing clients don't break silently.
- An object-storage implementation of `IImageStorage` (S3 / Azure Blob). Only a new class and a DI swap are needed; the URL contract stays the same.
- An admin UI for uploading. Image resizing, cropping or transformation. Multiple images per movie.
