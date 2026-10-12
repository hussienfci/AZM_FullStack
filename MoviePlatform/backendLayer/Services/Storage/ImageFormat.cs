namespace UserManagementApi.Services.Storage;

public enum ImageKind { Jpeg, Png, WebP }

public static class ImageFormat
{
    /// <summary>Detects the image type from the first bytes of the stream. The stream position is restored.</summary>
    public static ImageKind? Detect(Stream stream)
    {
        var original = stream.Position;
        var buf = new byte[12];
        var read = stream.Read(buf, 0, buf.Length);
        stream.Position = original;

        if (read < 3) return null;

        // JPEG: FF D8 FF
        if (buf[0] == 0xFF && buf[1] == 0xD8 && buf[2] == 0xFF)
            return ImageKind.Jpeg;

        // PNG: 89 50 4E 47 0D 0A 1A 0A (8 bytes)
        if (read >= 8
            && buf[0] == 0x89 && buf[1] == 0x50 && buf[2] == 0x4E && buf[3] == 0x47
            && buf[4] == 0x0D && buf[5] == 0x0A && buf[6] == 0x1A && buf[7] == 0x0A)
            return ImageKind.Png;

        // WebP: "RIFF" at 0-3 AND "WEBP" at 8-11
        if (read >= 12
            && buf[0] == 'R' && buf[1] == 'I' && buf[2] == 'F' && buf[3] == 'F'
            && buf[8] == 'W' && buf[9] == 'E' && buf[10] == 'B' && buf[11] == 'P')
            return ImageKind.WebP;

        return null;
    }

    public static string Extension(ImageKind kind) => kind switch
    {
        ImageKind.Jpeg => "jpg",
        ImageKind.Png => "png",
        _ => "webp"
    };

    public static string ContentType(ImageKind kind) => kind switch
    {
        ImageKind.Jpeg => "image/jpeg",
        ImageKind.Png => "image/png",
        _ => "image/webp"
    };
}
