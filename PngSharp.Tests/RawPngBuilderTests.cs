using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using Xunit;

namespace PngSharp.Tests;

public class RawPngBuilderTests
{
    private static IhdrChunkData MakeIhdr(
        uint width = 2, uint height = 2,
        ColorType colorType = ColorType.TrueColorWithAlpha,
        byte bitDepth = 8)
    {
        return new IhdrChunkData
        {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }

    [Fact]
    public void Build_ValidRgba_Succeeds()
    {
        var pixels = new byte[2 * 2 * 4]; // 2x2 RGBA
        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData(pixels)
            .Build();

        Assert.Equal(2u, png.Ihdr.Width);
        Assert.Equal(2u, png.Ihdr.Height);
        Assert.Equal(pixels, png.PixelData);
    }

    [Fact]
    public void Build_ZeroWidth_ThrowsInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(MakeIhdr(width: 0))
                .WithPixelData(new byte[16])
                .Build());
    }

    [Fact]
    public void Build_ZeroHeight_ThrowsInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(MakeIhdr(height: 0))
                .WithPixelData(new byte[16])
                .Build());
    }

    [Fact]
    public void Build_InvalidBitDepthForColorType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(MakeIhdr(colorType: ColorType.TrueColor, bitDepth: 1))
                .WithPixelData(new byte[12])
                .Build());
    }

    [Fact]
    public void Build_WrongPixelDataLength_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(MakeIhdr()) // 2x2 RGBA expects 16 bytes
                .WithPixelData(new byte[10])
                .Build());
    }
}
