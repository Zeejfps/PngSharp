using PngSharp.Api;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.IHDR;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class ChrmChunkTests
{
    [Fact]
    public void RoundTrip_Chrm_Preserved()
    {
        var chrm = new ChrmChunkData
        {
            WhitePointX = 31270,
            WhitePointY = 32900,
            RedX = 64000,
            RedY = 33000,
            GreenX = 30000,
            GreenY = 60000,
            BlueX = 15000,
            BlueY = 6000,
        };

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithChrm(chrm)
            .WithPixelData(new byte[4 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Chrm);
        Assert.Equal(chrm, decoded.Chrm.Value);
    }

    [Fact]
    public void RoundTrip_NoChrm_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Chrm);
    }
}
