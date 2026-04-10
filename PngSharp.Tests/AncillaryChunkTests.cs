using PngSharp.Api;
using PngSharp.Api.Exceptions;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using static PngSharp.Spec.Chunks.sRGB.RenderingIntent;
using PngSharp.Spec.Chunks.tIME;
using Xunit;

namespace PngSharp.Tests;

public class AncillaryChunkTests
{
    private static IRawPng RoundTrip(IRawPng png)
    {
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        return Png.DecodeFromByteArray(ms.ToArray());
    }

    #region cHRM

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

    #endregion

    #region tIME

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

    #endregion

    #region bKGD

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

    #endregion

    #region Multi-IDAT

    [Fact]
    public void Encode_LargeImage_ProducesMultipleIdatChunks()
    {
        // 64x64 RGBA = 16384 bytes uncompressed. Even after compression this should be > 8192
        var pixels = new byte[64 * 64 * 4];
        new Random(42).NextBytes(pixels);
        var png = Png.CreateRgba(64, 64, pixels);

        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var encoded = ms.ToArray();

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

        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var decoded = Png.DecodeFromByteArray(ms.ToArray());
        Assert.Equal(pixels, decoded.PixelData);
    }

    #endregion

    #region Chunk Ordering

    [Fact]
    public void RoundTrip_AllNewChunks_Together()
    {
        var chrm = new ChrmChunkData
        {
            WhitePointX = 31270, WhitePointY = 32900,
            RedX = 64000, RedY = 33000,
            GreenX = 30000, GreenY = 60000,
            BlueX = 15000, BlueY = 6000,
        };
        var time = new TimeChunkData { Year = 2026, Month = 4, Day = 9, Hour = 12, Minute = 0, Second = 0 };
        byte[] bkgdData = [0, 0, 0, 0, 0, 0];

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithChrm(chrm)
            .WithSrgb(new SrgbChunkData { RenderingIntent = Perceptual })
            .WithGama(new GammaChunkData { Value = 45455 })
            .WithBkgd(new BkgdChunkData { Data = bkgdData })
            .WithTime(time)
            .WithPixelData(new byte[4 * 4])
            .Build();

        var decoded = RoundTrip(png);

        Assert.Equal(chrm, decoded.Chrm!.Value);
        Assert.Equal(time, decoded.Time!.Value);
        Assert.Equal(bkgdData, decoded.Bkgd!.Value.Data);
        Assert.NotNull(decoded.Srgb);
        Assert.NotNull(decoded.Gama);
    }

    #endregion

    #region Helpers

    private static IhdrChunkData CreateIhdr(ColorType colorType, byte bitDepth = 8)
    {
        return new IhdrChunkData
        {
            Width = 2,
            Height = 2,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }

    private static int CountChunks(byte[] pngData, string chunkType)
    {
        var count = 0;
        var i = 8; // skip PNG signature
        while (i + 8 <= pngData.Length)
        {
            var length = (pngData[i] << 24) | (pngData[i + 1] << 16) | (pngData[i + 2] << 8) | pngData[i + 3];
            var type = System.Text.Encoding.ASCII.GetString(pngData, i + 4, 4);
            if (type == chunkType)
                count++;
            if (type == "IEND")
                break;
            i += 4 + 4 + length + 4; // length field + type + data + crc
        }
        return count;
    }

    #endregion
}
