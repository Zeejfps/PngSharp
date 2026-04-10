using PngSharp.Api;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class IdatChunkTests
{
    [Fact]
    public void Encode_LargeImage_ProducesMultipleIdatChunks()
    {
        // 64x64 RGBA = 16384 bytes uncompressed. Even after compression this should be > 8192
        var pixels = new byte[64 * 64 * 4];
        new Random(42).NextBytes(pixels);
        var png = Png.CreateRgba(64, 64, pixels);

        var encoded = Png.EncodeToByteArray(png);

        // Count IDAT chunks in the encoded bytes
        var idatCount = CountChunks(encoded, "IDAT");
        Assert.True(idatCount > 1, $"Expected multiple IDAT chunks, got {idatCount}");

        // Verify it still decodes correctly
        var decoded = Png.DecodeFromByteArray(encoded);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void Encode_SmallImage_StillDecodesCorrectly()
    {
        byte[] pixels = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 128, 128, 128, 255];
        var png = Png.CreateRgba(2, 2, pixels);

        var decoded = Png.DecodeFromByteArray(Png.EncodeToByteArray(png));
        Assert.Equal(pixels, decoded.PixelData);
    }
}
