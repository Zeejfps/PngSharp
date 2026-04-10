using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.tIME;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class TimeChunkTests
{
    [Fact]
    public void RoundTrip_Time_Preserved()
    {
        var time = new TimeChunkData
        {
            Year = 2026,
            Month = 4,
            Day = 9,
            Hour = 14,
            Minute = 30,
            Second = 0,
        };

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithTime(time)
            .WithPixelData(new byte[4 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Time);
        Assert.Equal(time, decoded.Time.Value);
    }

    [Fact]
    public void RoundTrip_NoTime_ReturnsNull()
    {
        var png = Png.CreateRgba(2, 2, new byte[2 * 2 * 4]);
        var decoded = RoundTrip(png);
        Assert.Null(decoded.Time);
    }

    [Fact]
    public void Builder_Time_InvalidMonth_Throws()
    {
        var time = new TimeChunkData { Year = 2026, Month = 13, Day = 1, Hour = 0, Minute = 0, Second = 0 };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithTime(time)
                .WithPixelData(new byte[4 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Time_InvalidHour_Throws()
    {
        var time = new TimeChunkData { Year = 2026, Month = 1, Day = 1, Hour = 24, Minute = 0, Second = 0 };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder()
                .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
                .WithTime(time)
                .WithPixelData(new byte[4 * 4])
                .Build());
    }

    [Fact]
    public void Builder_Time_LeapSecond_Allowed()
    {
        var time = new TimeChunkData { Year = 2026, Month = 6, Day = 30, Hour = 23, Minute = 59, Second = 60 };
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithTime(time)
            .WithPixelData(new byte[4 * 4])
            .Build();

        Assert.Equal(time, png.Time!.Value);
    }
}
