using UserManagementApi.Services.Storage;

namespace UserManagementApi.Tests;

public class ImageFormatTests
{
    private static MemoryStream MakeStream(params byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public void Detect_Jpeg_ReturnsJpeg()
    {
        using var s = MakeStream(0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01);
        Assert.Equal(ImageKind.Jpeg, ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_Png_ReturnsPng()
    {
        using var s = MakeStream(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D);
        Assert.Equal(ImageKind.Png, ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_WebP_ReturnsWebP()
    {
        // "RIFF....WEBP"
        using var s = MakeStream((byte)'R', (byte)'I', (byte)'F', (byte)'F',
                                  0x10, 0x00, 0x00, 0x00,
                                  (byte)'W', (byte)'E', (byte)'B', (byte)'P');
        Assert.Equal(ImageKind.WebP, ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_Gif_ReturnsNull()
    {
        using var s = MakeStream(0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00);
        Assert.Null(ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_TextFile_ReturnsNull()
    {
        using var s = MakeStream((byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
        Assert.Null(ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_TwoByteStream_ReturnsNull()
    {
        using var s = MakeStream(0xFF, 0xD8);
        Assert.Null(ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_RiffWave_ReturnsNull()
    {
        // "RIFF....WAVE" — has RIFF but not WEBP
        using var s = MakeStream((byte)'R', (byte)'I', (byte)'F', (byte)'F',
                                  0x10, 0x00, 0x00, 0x00,
                                  (byte)'W', (byte)'A', (byte)'V', (byte)'E');
        Assert.Null(ImageFormat.Detect(s));
    }

    [Fact]
    public void Detect_DoesNotChangeStreamPosition()
    {
        using var s = MakeStream(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D);
        s.Position = 3;
        ImageFormat.Detect(s);
        Assert.Equal(3, s.Position);
    }
}
