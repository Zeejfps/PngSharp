using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.tRNS;
using Xunit;

namespace PngSharp.Tests;

public class TrnsChunkTests
{
    [Fact]
    public void RoundTrip_Grayscale_TrnsPreserved()
    {
        // Transparent grey value = 0x0080 (big-endian)
        var trns = new TrnsChunkData { Data = [0x00, 0x80] };
        byte[] pixels = [0, 128, 255, 64];
        var png = CreatePngWithTrns(2, 2, ColorType.Grayscale, 8, trns, pixels);

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Trns);
        Assert.Equal(trns.Data, decoded.Trns.Value.Data);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_TrueColor_TrnsPreserved()
    {
        // Transparent RGB: R=0x00FF, G=0x0080, B=0x0000
        var trns = new TrnsChunkData { Data = [0x00, 0xFF, 0x00, 0x80, 0x00, 0x00] };
        byte[] pixels = [255, 128, 0, 0, 0, 255];
        var png = CreatePngWithTrns(2, 1, ColorType.TrueColor, 8, trns, pixels);

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Trns);
        Assert.Equal(trns.Data, decoded.Trns.Value.Data);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_IndexedColor_TrnsPreserved()
    {
        // 4-entry palette, alpha table: [255, 128, 0, 255]
        var plte = new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128] };
        var trns = new TrnsChunkData { Data = [255, 128, 0, 255] };
        byte[] pixels = [0, 1, 2, 3];
        var png = CreateIndexedPngWithTrns(2, 2, 8, plte, trns, pixels);

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Trns);
        Assert.Equal(trns.Data, decoded.Trns.Value.Data);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_IndexedColor_PartialTrns_Preserved()
    {
        // 4-entry palette but only 2 alpha values (rest default to 255)
        var plte = new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128] };
        var trns = new TrnsChunkData { Data = [0, 128] };
        byte[] pixels = [0, 1, 2, 3];
        var png = CreateIndexedPngWithTrns(2, 2, 8, plte, trns, pixels);

        var decoded = RoundTrip(png);

        Assert.NotNull(decoded.Trns);
        Assert.Equal(trns.Data, decoded.Trns.Value.Data);
    }

    [Fact]
    public void Build_GrayscaleWithAlpha_WithTrns_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.GrayscaleWithAlpha,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var trns = new TrnsChunkData { Data = [0x00, 0x80] };

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithTrns(trns).WithPixelData([0, 255]).Build());
    }

    [Fact]
    public void Build_TrueColorWithAlpha_WithTrns_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.TrueColorWithAlpha,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var trns = new TrnsChunkData { Data = [0x00, 0xFF, 0x00, 0x80, 0x00, 0x00] };

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithTrns(trns).WithPixelData([0, 0, 0, 255]).Build());
    }

    [Fact]
    public void Build_Grayscale_TrnsWrongLength_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.Grayscale,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var trns = new TrnsChunkData { Data = [0x00] }; // should be 2 bytes

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithTrns(trns).WithPixelData([0]).Build());
    }

    [Fact]
    public void Build_TrueColor_TrnsWrongLength_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.TrueColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var trns = new TrnsChunkData { Data = [0x00, 0xFF, 0x00] }; // should be 6 bytes

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithTrns(trns).WithPixelData([255, 0, 0]).Build());
    }

    private static IRawPng RoundTrip(IRawPng png)
    {
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        return Png.DecodeFromByteArray(ms.ToArray());
    }

    private static IRawPng CreatePngWithTrns(
        int width, int height, ColorType colorType, byte bitDepth,
        TrnsChunkData trns, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width, Height = (uint)height,
            BitDepth = bitDepth, ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        return Png.Builder().WithIhdr(ihdr).WithTrns(trns).WithPixelData(pixels).Build();
    }

    private static IRawPng CreateIndexedPngWithTrns(
        int width, int height, byte bitDepth,
        PlteChunkData plte, TrnsChunkData trns, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width, Height = (uint)height,
            BitDepth = bitDepth, ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        return Png.Builder().WithIhdr(ihdr).WithPlte(plte).WithTrns(trns).WithPixelData(pixels).Build();
    }
}
