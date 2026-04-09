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
        var ihdr = new IhdrChunkData
        {
            Width = 1,
            Height = 1,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };

        Assert.Equal(expected, ihdr.GetBytesPerPixel());
    }
}
