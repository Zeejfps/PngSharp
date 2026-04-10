using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class PlteChunkTests
{
    [Fact]
    public void RoundTrip_IndexedColor_PalettePreserved()
    {
        // 2-entry palette: red and blue
        var plte = new PlteChunkData { Entries = [255, 0, 0, 0, 0, 255] };
        // 2x2 image, pixel indices: 0, 1, 1, 0
        byte[] pixels = [0, 1, 1, 0];
        var png = CreateIndexedPng(2, 2, 8, plte, pixels);

        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.NotNull(decoded.Plte);
        Assert.Equal(plte.Entries, decoded.Plte.Value.Entries);
        Assert.Equal(2, decoded.Plte.Value.EntryCount);
    }

    [Fact]
    public void RoundTrip_IndexedColor_4Bit_PalettePreserved()
    {
        // 4-entry palette
        var plte = new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128] };
        byte[] pixels = [0, 1, 2, 3];
        var png = CreateIndexedPng(2, 2, 4, plte, pixels);

        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.NotNull(decoded.Plte);
        Assert.Equal(plte.Entries, decoded.Plte.Value.Entries);
    }

    [Fact]
    public void RoundTrip_IndexedColor_1Bit_PalettePreserved()
    {
        // 2-entry palette: black and white
        var plte = new PlteChunkData { Entries = [0, 0, 0, 255, 255, 255] };
        byte[] pixels = [0, 1, 1, 0, 0, 1, 1, 0];
        var png = CreateIndexedPng(8, 1, 1, plte, pixels);

        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.NotNull(decoded.Plte);
        Assert.Equal(plte.Entries, decoded.Plte.Value.Entries);
    }

    [Fact]
    public void Build_IndexedColor_WithoutPlte_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithPixelData([0]).Build());
    }

    [Fact]
    public void Build_Grayscale_WithPlte_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.Grayscale,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var plte = new PlteChunkData { Entries = [0, 0, 0] };

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithPixelData([0]).WithPlte(plte).Build());
    }

    [Fact]
    public void Build_PlteEntriesExceedBitDepth_Throws()
    {
        // 1-bit indexed allows max 2 entries, providing 3 should fail
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 1,
            ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var plte = new PlteChunkData { Entries = [0, 0, 0, 255, 255, 255, 128, 128, 128] };

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithPixelData([0]).WithPlte(plte).Build());
    }

    [Fact]
    public void Build_PlteEntriesNotDivisibleBy3_Throws()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        var plte = new PlteChunkData { Entries = [0, 0] }; // not divisible by 3

        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(ihdr).WithPixelData([0]).WithPlte(plte).Build());
    }

    private static IRawPng CreateIndexedPng(int width, int height, byte bitDepth, PlteChunkData plte, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width,
            Height = (uint)height,
            BitDepth = bitDepth,
            ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        return Png.Builder().WithIhdr(ihdr).WithPlte(plte).WithPixelData(pixels).Build();
    }
}
