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
