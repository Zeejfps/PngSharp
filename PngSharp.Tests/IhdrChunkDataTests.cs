using PngSharp.Spec.Chunks.IHDR;
using Xunit;

namespace PngSharp.Tests;

public class IhdrChunkDataTests
{
    [Theory]
    [InlineData(ColorType.Grayscale, 8, 1)]
    [InlineData(ColorType.TrueColor, 8, 3)]
    [InlineData(ColorType.IndexedColor, 8, 1)]
    [InlineData(ColorType.GrayscaleWithAlpha, 8, 2)]
    [InlineData(ColorType.TrueColorWithAlpha, 8, 4)]
    [InlineData(ColorType.Grayscale, 1, 1)]
    public void GetBytesPerPixel_ReturnsExpected(ColorType colorType, byte bitDepth, int expected)
    {
        var ihdr = MakeIhdr(1, colorType, bitDepth);
        Assert.Equal(expected, ihdr.GetBytesPerPixel());
    }

    [Theory]
    [InlineData(ColorType.Grayscale, 1, 1)]
    [InlineData(ColorType.Grayscale, 2, 2)]
    [InlineData(ColorType.Grayscale, 4, 4)]
    [InlineData(ColorType.Grayscale, 8, 8)]
    [InlineData(ColorType.Grayscale, 16, 16)]
    [InlineData(ColorType.TrueColor, 8, 24)]
    [InlineData(ColorType.TrueColor, 16, 48)]
    [InlineData(ColorType.IndexedColor, 1, 1)]
    [InlineData(ColorType.IndexedColor, 2, 2)]
    [InlineData(ColorType.IndexedColor, 4, 4)]
    [InlineData(ColorType.IndexedColor, 8, 8)]
    [InlineData(ColorType.GrayscaleWithAlpha, 8, 16)]
    [InlineData(ColorType.GrayscaleWithAlpha, 16, 32)]
    [InlineData(ColorType.TrueColorWithAlpha, 8, 32)]
    [InlineData(ColorType.TrueColorWithAlpha, 16, 64)]
    public void GetBitsPerPixel_ReturnsExpected(ColorType colorType, byte bitDepth, int expected)
    {
        var ihdr = MakeIhdr(1, colorType, bitDepth);
        Assert.Equal(expected, ihdr.GetBitsPerPixel());
    }

    [Theory]
    // 1-bit grayscale: 1 pixel = ceil(1/8) = 1 byte
    [InlineData(ColorType.Grayscale, 1, 1u, 1)]
    // 1-bit grayscale: 8 pixels pack exactly into 1 byte
    [InlineData(ColorType.Grayscale, 1, 8u, 1)]
    // 1-bit grayscale: 9 pixels = ceil(9/8) = 2 bytes
    [InlineData(ColorType.Grayscale, 1, 9u, 2)]
    // 2-bit grayscale: 4 pixels = ceil(8/8) = 1 byte
    [InlineData(ColorType.Grayscale, 2, 4u, 1)]
    // 2-bit grayscale: 5 pixels = ceil(10/8) = 2 bytes
    [InlineData(ColorType.Grayscale, 2, 5u, 2)]
    // 4-bit grayscale: 2 pixels = ceil(8/8) = 1 byte
    [InlineData(ColorType.Grayscale, 4, 2u, 1)]
    // 4-bit grayscale: 3 pixels = ceil(12/8) = 2 bytes
    [InlineData(ColorType.Grayscale, 4, 3u, 2)]
    // 1-bit indexed: 7 pixels = ceil(7/8) = 1 byte
    [InlineData(ColorType.IndexedColor, 1, 7u, 1)]
    // 8-bit grayscale: width * 1
    [InlineData(ColorType.Grayscale, 8, 4u, 4)]
    // 8-bit TrueColor: width * 3
    [InlineData(ColorType.TrueColor, 8, 4u, 12)]
    // 16-bit grayscale: width * 2
    [InlineData(ColorType.Grayscale, 16, 4u, 8)]
    // 16-bit TrueColor: width * 6
    [InlineData(ColorType.TrueColor, 16, 4u, 24)]
    // 16-bit TrueColorWithAlpha: width * 8
    [InlineData(ColorType.TrueColorWithAlpha, 16, 4u, 32)]
    public void GetScanlineByteWidth_ReturnsExpected(ColorType colorType, byte bitDepth, uint width, int expected)
    {
        var ihdr = MakeIhdr(width, colorType, bitDepth);
        Assert.Equal(expected, ihdr.GetScanlineByteWidth());
    }

    private static IhdrChunkData MakeIhdr(uint width, ColorType colorType, byte bitDepth)
    {
        return new IhdrChunkData
        {
            Width = width,
            Height = 1,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }
}
