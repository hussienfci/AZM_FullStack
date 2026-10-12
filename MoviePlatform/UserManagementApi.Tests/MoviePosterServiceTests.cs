using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UserManagementApi.Data;
using UserManagementApi.Mappings;
using UserManagementApi.Models;
using UserManagementApi.Models.DTOs;
using UserManagementApi.Services;
using UserManagementApi.Services.Storage;

namespace UserManagementApi.Tests;

public class MoviePosterServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly AppDbContext _context;
    private readonly MovieService _service;
    private readonly LocalImageStorage _storage;

    private static IMapper BuildMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    public MoviePosterServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(dbOptions);

        var mapper = BuildMapper();

        var storageOptions = Options.Create(new ImageStorageOptions { RootPath = _tempDir, MaxPosterBytes = 5 * 1024 * 1024 });
        var env = new FakeEnv();
        _storage = new LocalImageStorage(storageOptions, env, NullLogger<LocalImageStorage>.Instance);

        _service = new MovieService(
            _context,
            mapper,
            NullLogger<MovieService>.Instance,
            _storage,
            storageOptions);
    }

    public void Dispose()
    {
        _context.Dispose();
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    // Helper: a valid PNG FormFile
    private static IFormFile PngFile(long size = 100)
    {
        var bytes = new byte[size];
        // PNG magic bytes
        bytes[0] = 0x89; bytes[1] = 0x50; bytes[2] = 0x4E; bytes[3] = 0x47;
        bytes[4] = 0x0D; bytes[5] = 0x0A; bytes[6] = 0x1A; bytes[7] = 0x0A;
        var ms = new MemoryStream(bytes);
        return new FormFile(ms, 0, size, "file", "test.png") { Headers = new HeaderDictionary() };
    }

    private static IFormFile GifFile()
    {
        var bytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00 };
        var ms = new MemoryStream(bytes);
        return new FormFile(ms, 0, bytes.Length, "file", "test.gif") { Headers = new HeaderDictionary() };
    }

    private async Task<Movie> AddMovieAsync(string? posterUrl = null)
    {
        var movie = new Movie { Title = "Test Movie", Genre = "Action", IsActive = true, PosterUrl = posterUrl };
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    [Fact]
    public async Task SetPoster_AttachesPosterAndReturnsDto()
    {
        var movie = await AddMovieAsync();
        var result = await _service.SetPosterAsync(movie.Id, PngFile());

        Assert.True(result.Success);
        Assert.NotNull(result.Data?.PosterUrl);
        Assert.Matches(@"^/api/media/posters/[0-9a-f]{32}\.png$", result.Data!.PosterUrl!);

        var dbMovie = await _context.Movies.FindAsync(movie.Id);
        Assert.Equal(result.Data.PosterUrl, dbMovie!.PosterUrl);
    }

    [Fact]
    public async Task SetPoster_UnknownMovie_ReturnsNotFound()
    {
        var postersDir = Path.Combine(_tempDir, "posters");
        var filesBefore = Directory.GetFiles(postersDir).Length;

        var result = await _service.SetPosterAsync(99999, PngFile());

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(filesBefore, Directory.GetFiles(postersDir).Length);
    }

    [Fact]
    public async Task SetPoster_InvalidFormat_ReturnsError_AndPosterUnchanged()
    {
        var movie = await AddMovieAsync(posterUrl: null);

        var result = await _service.SetPosterAsync(movie.Id, GifFile());

        Assert.False(result.Success);
        Assert.Contains("Unsupported image format", result.Message);

        var dbMovie = await _context.Movies.FindAsync(movie.Id);
        Assert.Null(dbMovie!.PosterUrl);
    }

    [Fact]
    public async Task SetPoster_TooLarge_ReturnsError()
    {
        var smallLimitOptions = Options.Create(new ImageStorageOptions { RootPath = _tempDir, MaxPosterBytes = 10 });
        var svc = new MovieService(
            _context,
            BuildMapper(),
            NullLogger<MovieService>.Instance,
            _storage,
            smallLimitOptions);

        var movie = await AddMovieAsync();
        var result = await svc.SetPosterAsync(movie.Id, PngFile(size: 20));

        Assert.False(result.Success);
        Assert.Contains("too large", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetPoster_NullFile_ReturnsNoFileUploaded()
    {
        var movie = await AddMovieAsync();
        var result = await _service.SetPosterAsync(movie.Id, null);

        Assert.False(result.Success);
        Assert.Equal("No file was uploaded", result.Message);
    }

    [Fact]
    public async Task SetPoster_Replace_DeletesOldManagedFile()
    {
        var movie = await AddMovieAsync();

        var first = await _service.SetPosterAsync(movie.Id, PngFile());
        var firstUrl = first.Data!.PosterUrl!;
        var firstName = firstUrl[LocalImageStorage.PosterUrlPrefix.Length..];
        var firstPath = Path.Combine(_tempDir, "posters", firstName);
        Assert.True(File.Exists(firstPath));

        var second = await _service.SetPosterAsync(movie.Id, PngFile());
        Assert.True(second.Success);

        Assert.False(File.Exists(firstPath));
        var secondUrl = second.Data!.PosterUrl!;
        var secondName = secondUrl[LocalImageStorage.PosterUrlPrefix.Length..];
        Assert.True(File.Exists(Path.Combine(_tempDir, "posters", secondName)));
    }

    [Fact]
    public async Task SetPoster_ExternalPosterUrl_KeepsItAndDoesNotThrow()
    {
        var movie = await AddMovieAsync(posterUrl: "https://example.com/p.jpg");

        var result = await _service.SetPosterAsync(movie.Id, PngFile());

        Assert.True(result.Success);
        Assert.True(_storage.IsManagedPosterUrl(result.Data!.PosterUrl));
    }

    [Fact]
    public async Task RemovePoster_ClearsUrlAndDeletesFile()
    {
        var movie = await AddMovieAsync();
        var upload = await _service.SetPosterAsync(movie.Id, PngFile());
        var url = upload.Data!.PosterUrl!;
        var fileName = url[LocalImageStorage.PosterUrlPrefix.Length..];
        var filePath = Path.Combine(_tempDir, "posters", fileName);
        Assert.True(File.Exists(filePath));

        var remove = await _service.RemovePosterAsync(movie.Id);

        Assert.True(remove.Success);
        Assert.Null(remove.Data?.PosterUrl);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task RemovePoster_Twice_BothSucceed()
    {
        var movie = await AddMovieAsync();
        await _service.SetPosterAsync(movie.Id, PngFile());

        var first = await _service.RemovePosterAsync(movie.Id);
        var second = await _service.RemovePosterAsync(movie.Id);

        Assert.True(first.Success);
        Assert.True(second.Success);
    }

    [Fact]
    public async Task UpdateAsync_WithExternalHttpsUrl_Succeeds()
    {
        var movie = await AddMovieAsync();
        var dto = new UpdateMovieDto
        {
            Title = movie.Title,
            Genre = movie.Genre,
            IsActive = true,
            PosterUrl = "https://example.com/p.jpg"
        };

        var result = await _service.UpdateAsync(movie.Id, dto);

        Assert.True(result.Success);
        Assert.Equal("https://example.com/p.jpg", result.Data!.PosterUrl);
    }

    [Fact]
    public async Task UpdateAsync_WithSomeoneElsesManagedUrl_IsRejected()
    {
        var movie = await AddMovieAsync();
        var otherManagedUrl = "/api/media/posters/" + new string('b', 32) + ".jpg";

        var dto = new UpdateMovieDto { Title = movie.Title, Genre = movie.Genre, IsActive = true, PosterUrl = otherManagedUrl };
        var result = await _service.UpdateAsync(movie.Id, dto);

        Assert.False(result.Success);
        Assert.Contains("PosterUrl must be an absolute http(s) URL", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_EchoingOwnManagedUrl_Succeeds_AndKeepsFile()
    {
        var movie = await AddMovieAsync();
        var upload = await _service.SetPosterAsync(movie.Id, PngFile());
        var ownUrl = upload.Data!.PosterUrl!;
        var fileName = ownUrl[LocalImageStorage.PosterUrlPrefix.Length..];
        var filePath = Path.Combine(_tempDir, "posters", fileName);

        var dto = new UpdateMovieDto { Title = movie.Title, Genre = movie.Genre, IsActive = true, PosterUrl = ownUrl };
        var result = await _service.UpdateAsync(movie.Id, dto);

        Assert.True(result.Success);
        Assert.Equal(ownUrl, result.Data!.PosterUrl);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteAsync_DeletesManagedPosterFile()
    {
        var movie = await AddMovieAsync();
        var upload = await _service.SetPosterAsync(movie.Id, PngFile());
        var url = upload.Data!.PosterUrl!;
        var fileName = url[LocalImageStorage.PosterUrlPrefix.Length..];
        var filePath = Path.Combine(_tempDir, "posters", fileName);
        Assert.True(File.Exists(filePath));

        await _service.DeleteAsync(movie.Id);

        Assert.False(File.Exists(filePath));
    }
}

file sealed class FakeEnv : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
{
    public string WebRootPath { get; set; } = string.Empty;
    public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "Test";
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Test";
}
