using PngSharp.Api;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class BkgdChunkTests
{
    [Fact]
    public void RoundTrip_Bkgd_TrueColor_Preserved()
    {
        byte[] bkgdData = [0, 255, 0, 128, 0, 0]; // R=255, G=128, B=0
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithBkgd(new BkgdChunkData { Data = bkgdData })
            .WithPixelData(new byte[4 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Bkgd);
        Assert.Equal(bkgdData, decoded.Bkgd.Value.Data);
    }

    [Fact]
    public void RoundTrip_Bkgd_Grayscale_Preserved()
    {
        byte[] bkgdData = [0, 128]; // gray = 128
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.Grayscale))
            .WithBkgd(new BkgdChunkData { Data = bkgdData })
            .WithPixelData(new byte[2 * 2])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Bkgd);
        Assert.Equal(bkgdData, decoded.Bkgd.Value.Data);
    }

    [Fact]
    public void RoundTrip_Bkgd_Indexed_Preserved()
    {
        byte[] bkgdData = [1]; // palette index 1
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPlte(new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0] })
            .WithBkgd(new BkgdChunkData { Data = bkgdData })
            .WithPixelData(new byte[2 * 2])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Bkgd);
        Assert.Equal(bkgdData, decoded.Bkgd.Value.Data);
    }

    [Fact]
    public void Builder_Bkgd_WrongSizeForColorType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithBkgd(new BkgdChunkData { Data = [0, 128] }) // should be 6 bytes
                .WithPixelData(new byte[4 * 4])
                .Build());
    }

    [Fact]
    public void RoundTrip_NoBkgd_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Bkgd);
    }
}
