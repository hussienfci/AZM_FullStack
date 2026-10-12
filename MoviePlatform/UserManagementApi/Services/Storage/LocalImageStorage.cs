using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace UserManagementApi.Services.Storage;

public class LocalImageStorage : IImageStorage
{
    public const string PosterUrlPrefix = "/api/media/posters/";

    private static readonly Regex FileNamePattern = new(
        @"^[0-9a-f]{32}\.(jpg|png|webp)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string _postersDir;
    private readonly ILogger<LocalImageStorage> _logger;

    public LocalImageStorage(
        IOptions<ImageStorageOptions> options,
        IWebHostEnvironment env,
        ILogger<LocalImageStorage> logger)
    {
        _logger = logger;
        var rootPath = options.Value.RootPath;
        var resolved = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(env.ContentRootPath, rootPath);
        _postersDir = Path.GetFullPath(Path.Combine(resolved, "posters"));
        Directory.CreateDirectory(_postersDir);
    }

    public async Task<string> SavePosterAsync(Stream content, ImageKind kind, CancellationToken ct = default)
    {
        var name = $"{Guid.NewGuid():N}.{ImageFormat.Extension(kind)}";
        var path = Path.Combine(_postersDir, name);
        await using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fs, ct);
        return PosterUrlPrefix + name;
    }

    public bool IsManagedPosterUrl(string? url)
    {
        if (url is null) return false;
        if (!url.StartsWith(PosterUrlPrefix, StringComparison.Ordinal)) return false;
        var remainder = url[PosterUrlPrefix.Length..];
        return FileNamePattern.IsMatch(remainder);
    }

    public Task<bool> DeletePosterAsync(string? url, CancellationToken ct = default)
    {
        if (!IsManagedPosterUrl(url)) return Task.FromResult(false);
        var fileName = url![PosterUrlPrefix.Length..];
        var path = Path.Combine(_postersDir, fileName);

        // defence-in-depth: assert no traversal
        if (!Path.GetFullPath(path).StartsWith(_postersDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        try
        {
            if (!File.Exists(path)) return Task.FromResult(false);
            File.Delete(path);
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not delete poster file {Path}", path);
            return Task.FromResult(false);
        }
    }

    public (Stream Content, string ContentType)? OpenPoster(string fileName)
    {
        if (!FileNamePattern.IsMatch(fileName)) return null;

        var path = Path.Combine(_postersDir, fileName);

        // defence-in-depth: assert no traversal
        if (!Path.GetFullPath(path).StartsWith(_postersDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!File.Exists(path)) return null;

        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        var kind = ext switch
        {
            "jpg" => ImageKind.Jpeg,
            "png" => ImageKind.Png,
            _ => ImageKind.WebP
        };

        return (File.OpenRead(path), ImageFormat.ContentType(kind));
    }
}
