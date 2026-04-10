using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.eXIf;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class ExifChunkTests
{
    [Fact]
    public void RoundTrip_Exif_BigEndian_Preserved()
    {
        byte[] exifData = [0x4D, 0x4D, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithExif(new ExifChunkData { Data = exifData })
            .WithPixelData(new byte[2 * 2 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Exif);
        Assert.Equal(exifData, decoded.Exif.Value.Data);
    }

    [Fact]
    public void RoundTrip_Exif_LittleEndian_Preserved()
    {
        byte[] exifData = [0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithExif(new ExifChunkData { Data = exifData })
            .WithPixelData(new byte[2 * 2 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Exif);
        Assert.Equal(exifData, decoded.Exif.Value.Data);
    }

    [Fact]
    public void Builder_Exif_TooShort_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithExif(new ExifChunkData { Data = [0x4D, 0x4D, 0x00] }) // 3 bytes, min is 4
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Exif_InvalidByteOrderMark_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithExif(new ExifChunkData { Data = [0x00, 0x00, 0x00, 0x00] })
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void RoundTrip_NoExif_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Exif);
    }
}
