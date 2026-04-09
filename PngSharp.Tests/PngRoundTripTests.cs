using PngSharp.Api;
using Xunit;

namespace PngSharp.Tests;

public class PngRoundTripTests
{
    private static IRawPng RoundTrip(IRawPng png)
    {
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var bytes = ms.ToArray();
        return Png.DecodeFromByteArray(bytes);
    }

    [Fact]
    public void RoundTrip_Rgba_PixelDataPreserved()
    {
        byte[] pixels = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 128, 128, 128, 255];
        var png = Png.CreateRgba(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_Rgb_PixelDataPreserved()
    {
        byte[] pixels = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128];
        var png = Png.CreateRgb(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_Grayscale_PixelDataPreserved()
    {
        byte[] pixels = [0, 85, 170, 255];
        var png = Png.CreateGrayscale(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_GrayscaleWithAlpha_PixelDataPreserved()
    {
        byte[] pixels = [0, 255, 85, 200, 170, 128, 255, 50];
        var png = Png.CreateGrayscaleWithAlpha(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void Encode_ProducesValidPngStructure()
    {
        // RFC 2083 §3.1: PNG signature is 8 bytes: 137 80 78 71 13 10 26 10
        // RFC 2083 §4.1.1: First chunk must be IHDR with 13 bytes of data
        var png = Png.CreateRgba(2, 2, new byte[16]);
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var bytes = ms.ToArray();

        // PNG signature
        byte[] expectedSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(expectedSignature, bytes[..8]);

        // IHDR chunk: length = 13 (big-endian), type = "IHDR"
        Assert.Equal(0x00, bytes[8]);
        Assert.Equal(0x00, bytes[9]);
        Assert.Equal(0x00, bytes[10]);
        Assert.Equal(0x0D, bytes[11]);
        Assert.Equal((byte)'I', bytes[12]);
        Assert.Equal((byte)'H', bytes[13]);
        Assert.Equal((byte)'D', bytes[14]);
        Assert.Equal((byte)'R', bytes[15]);

        // IHDR data: width=2, height=2 (big-endian uint32)
        Assert.Equal(0x00, bytes[16]);
        Assert.Equal(0x00, bytes[17]);
        Assert.Equal(0x00, bytes[18]);
        Assert.Equal(0x02, bytes[19]);
        Assert.Equal(0x00, bytes[20]);
        Assert.Equal(0x00, bytes[21]);
        Assert.Equal(0x00, bytes[22]);
        Assert.Equal(0x02, bytes[23]);

        // Bit depth=8, color type=6 (RGBA)
        Assert.Equal(8, bytes[24]);
        Assert.Equal(6, bytes[25]);
    }

    [Fact]
    public void Decode_Test2x2Asset_Succeeds()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "test_2x2.png");
        var bytes = File.ReadAllBytes(path);
        var png = Png.DecodeFromByteArray(bytes);

        Assert.Equal(2u, png.Ihdr.Width);
        Assert.Equal(2u, png.Ihdr.Height);
        Assert.NotNull(png.PixelData);
        var expectedLength = 2 * 2 * png.Ihdr.GetBytesPerPixel();
        Assert.Equal(expectedLength, png.PixelData.Length);
    }
}
