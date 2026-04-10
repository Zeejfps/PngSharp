using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.iCCP;
using PngSharp.Spec.Chunks.sRGB;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class IccpChunkTests
{
    private static readonly byte[] SampleProfile = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08];

    [Fact]
    public void RoundTrip_Iccp_Preserved()
    {
        var iccp = IccpChunkData.Create("TestProfile", SampleProfile);
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithIccp(iccp)
            .WithPixelData(new byte[2 * 2 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Iccp);
        Assert.Equal("TestProfile", decoded.Iccp.Value.ProfileName);
        Assert.Equal(SampleProfile, decoded.Iccp.Value.DecompressProfile());
    }

    [Fact]
    public void RoundTrip_Iccp_CompressedDataPreserved()
    {
        var iccp = IccpChunkData.Create("MyProfile", SampleProfile);
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithIccp(iccp)
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Iccp);
        Assert.Equal(iccp.CompressedProfile, decoded.Iccp.Value.CompressedProfile);
    }

    [Fact]
    public void Builder_Iccp_WithSrgb_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithIccp(IccpChunkData.Create("Test", SampleProfile))
                .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Iccp_EmptyProfileName_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithIccp(new IccpChunkData { ProfileName = "", CompressedProfile = [1, 2, 3] })
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Iccp_ProfileNameTooLong_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithIccp(new IccpChunkData { ProfileName = new string('A', 80), CompressedProfile = [1, 2, 3] })
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Iccp_EmptyCompressedData_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithIccp(new IccpChunkData { ProfileName = "Test", CompressedProfile = [] })
                .WithPixelData(new byte[2 * 2 * 4])
                .Build());
    }

    [Fact]
    public void RoundTrip_NoIccp_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Iccp);
    }
}
