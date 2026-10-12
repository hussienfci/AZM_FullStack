using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using UserManagementApi.Services.Storage;

namespace UserManagementApi.Tests;

public class LocalImageStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly LocalImageStorage _storage;

    public LocalImageStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var options = Options.Create(new ImageStorageOptions { RootPath = _tempDir });
        var env = new FakeWebHostEnvironment();
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalImageStorage>.Instance;
        _storage = new LocalImageStorage(options, env, logger);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    [Fact]
    public async Task SavePosterAsync_ReturnsManagedUrl_AndFileExists()
    {
        var content = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 });
        var url = await _storage.SavePosterAsync(content, ImageKind.Png);

        Assert.Matches(@"^/api/media/posters/[0-9a-f]{32}\.png$", url);

        var fileName = url[LocalImageStorage.PosterUrlPrefix.Length..];
        var filePath = Path.Combine(_tempDir, "posters", fileName);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task IsManagedPosterUrl_TrueForSavedUrl()
    {
        var content = new MemoryStream(new byte[10]);
        var url = await _storage.SavePosterAsync(content, ImageKind.Jpeg);
        Assert.True(_storage.IsManagedPosterUrl(url));
    }

    [Theory]
    [InlineData("https://example.com/a.jpg")]
    [InlineData("/api/media/posters/../x.jpg")]
    [InlineData("/api/media/posters/ABC.jpg")]
    [InlineData(null)]
    [InlineData("")]
    public void IsManagedPosterUrl_FalseForNonManagedUrls(string? url)
    {
        Assert.False(_storage.IsManagedPosterUrl(url));
    }

    [Fact]
    public async Task DeletePosterAsync_DeletesManagedFile()
    {
        var content = new MemoryStream(new byte[10]);
        var url = await _storage.SavePosterAsync(content, ImageKind.Png);

        var fileName = url[LocalImageStorage.PosterUrlPrefix.Length..];
        var path = Path.Combine(_tempDir, "posters", fileName);
        Assert.True(File.Exists(path));

        var result = await _storage.DeletePosterAsync(url);
        Assert.True(result);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task DeletePosterAsync_ReturnsFalseForExternalUrl()
    {
        var result = await _storage.DeletePosterAsync("https://example.com/a.jpg");
        Assert.False(result);
    }

    [Fact]
    public async Task DeletePosterAsync_ReturnsFalseForMissingManagedFile()
    {
        // A valid-format managed URL for a file that doesn't exist
        var fakeUrl = "/api/media/posters/" + new string('a', 32) + ".jpg";
        var result = await _storage.DeletePosterAsync(fakeUrl);
        Assert.False(result);
    }

    [Fact]
    public void OpenPoster_PathTraversal_ReturnsNull()
    {
        Assert.Null(_storage.OpenPoster("../appsettings.json"));
    }

    [Fact]
    public void OpenPoster_GifExtension_ReturnsNull()
    {
        Assert.Null(_storage.OpenPoster("x.gif"));
    }
}

file sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = string.Empty;
    public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "Test";
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Test";
}
