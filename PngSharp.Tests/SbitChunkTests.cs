using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sBIT;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class SbitChunkTests
{
    [Fact]
    public void RoundTrip_Sbit_TrueColorWithAlpha_Preserved()
    {
        byte[] sbitData = [8, 8, 8, 8];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithSbit(new SbitChunkData { Data = sbitData })
            .WithPixelData(new byte[2 * 2 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Sbit);
        Assert.Equal(sbitData, decoded.Sbit.Value.Data);
    }

    [Fact]
    public void RoundTrip_Sbit_TrueColor_Preserved()
    {
        byte[] sbitData = [5, 6, 5];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithSbit(new SbitChunkData { Data = sbitData })
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Sbit);
        Assert.Equal(sbitData, decoded.Sbit.Value.Data);
    }

    [Fact]
    public void RoundTrip_Sbit_Grayscale_Preserved()
    {
        byte[] sbitData = [5];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.Grayscale))
            .WithSbit(new SbitChunkData { Data = sbitData })
            .WithPixelData(new byte[2 * 2])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Sbit);
        Assert.Equal(sbitData, decoded.Sbit.Value.Data);
    }

    [Fact]
    public void RoundTrip_Sbit_GrayscaleWithAlpha_Preserved()
    {
        byte[] sbitData = [5, 8];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.GrayscaleWithAlpha))
            .WithSbit(new SbitChunkData { Data = sbitData })
            .WithPixelData(new byte[2 * 2 * 2])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Sbit);
        Assert.Equal(sbitData, decoded.Sbit.Value.Data);
    }

    [Fact]
    public void RoundTrip_Sbit_IndexedColor_Preserved()
    {
        byte[] sbitData = [5, 6, 5];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPlte(new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128] })
            .WithSbit(new SbitChunkData { Data = sbitData })
            .WithPixelData(new byte[2 * 2])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Sbit);
        Assert.Equal(sbitData, decoded.Sbit.Value.Data);
    }

    [Fact]
    public void Builder_Sbit_WrongSizeForColorType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColor))
                .WithSbit(new SbitChunkData { Data = [8, 8] }) // should be 3 bytes
                .WithPixelData(new byte[2 * 2 * 3])
                .Build());
    }

    [Fact]
    public void Builder_Sbit_ZeroValue_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColor))
                .WithSbit(new SbitChunkData { Data = [0, 8, 8] })
                .WithPixelData(new byte[2 * 2 * 3])
                .Build());
    }

    [Fact]
    public void Builder_Sbit_ExceedsBitDepth_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.Grayscale))
                .WithSbit(new SbitChunkData { Data = [9] }) // 8-bit depth, max is 8
                .WithPixelData(new byte[2 * 2])
                .Build());
    }

    [Fact]
    public void RoundTrip_NoSbit_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Sbit);
    }
}
