using PngSharp.Api;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using Xunit;

namespace PngSharp.Tests;

public class Adam7Tests
{
    // --- Dimension calculation tests ---

    [Fact]
    public void GetPassDimensions_1x1_OnlyPass1HasPixels()
    {
        Assert.Equal(1, Adam7.GetPassWidth(1, 0));
        Assert.Equal(1, Adam7.GetPassHeight(1, 0));

        for (var pass = 1; pass < Adam7.PassCount; pass++)
        {
            Assert.True(
                Adam7.GetPassWidth(1, pass) == 0 || Adam7.GetPassHeight(1, pass) == 0,
                $"Pass {pass + 1} should be empty for 1x1 image");
        }
    }

    [Fact]
    public void GetPassDimensions_8x8_AllPassesNonEmpty()
    {
        for (var pass = 0; pass < Adam7.PassCount; pass++)
        {
            Assert.True(Adam7.GetPassWidth(8, pass) > 0, $"Pass {pass + 1} width should be > 0 for 8x8");
            Assert.True(Adam7.GetPassHeight(8, pass) > 0, $"Pass {pass + 1} height should be > 0 for 8x8");
        }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    [InlineData(7, 7)]
    [InlineData(8, 8)]
    [InlineData(9, 5)]
    [InlineData(16, 16)]
    [InlineData(100, 100)]
    public void TotalPixelCount_EqualsImageSize(int width, int height)
    {
        var totalPixels = 0;
        for (var pass = 0; pass < Adam7.PassCount; pass++)
        {
            totalPixels += Adam7.GetPassWidth(width, pass) * Adam7.GetPassHeight(height, pass);
        }
        Assert.Equal(width * height, totalPixels);
    }

    [Fact]
    public void GetPassScanlineByteWidth_SubBytePadding()
    {
        // 3 pixels at 1 bpp = 3 bits -> 1 byte
        Assert.Equal(1, Adam7.GetPassScanlineByteWidth(3, 1));
        // 9 pixels at 1 bpp = 9 bits -> 2 bytes
        Assert.Equal(2, Adam7.GetPassScanlineByteWidth(9, 1));
        // 3 pixels at 4 bpp = 12 bits -> 2 bytes
        Assert.Equal(2, Adam7.GetPassScanlineByteWidth(3, 4));
        // 2 pixels at 8 bpp = 16 bits -> 2 bytes
        Assert.Equal(2, Adam7.GetPassScanlineByteWidth(2, 8));
    }

    // --- Round-trip tests ---

    [Fact]
    public void RoundTrip_Adam7_Rgba_8bit()
    {
        var pixels = CreateTestPixels(8, 8, 4);
        var png = CreateAdam7Png(8, 8, ColorType.TrueColorWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(InterlaceMethod.Adam7, decoded.Ihdr.InterlaceMethod);
        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_Rgb_8bit()
    {
        var pixels = CreateTestPixels(8, 8, 3);
        var png = CreateAdam7Png(8, 8, ColorType.TrueColor, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_Grayscale_8bit()
    {
        var pixels = CreateTestPixels(8, 8, 1);
        var png = CreateAdam7Png(8, 8, ColorType.Grayscale, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_GrayscaleAlpha_8bit()
    {
        var pixels = CreateTestPixels(8, 8, 2);
        var png = CreateAdam7Png(8, 8, ColorType.GrayscaleWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_Rgba_16bit()
    {
        var pixels = CreateTestPixels(8, 8, 8); // 4 channels * 2 bytes
        var png = CreateAdam7Png(8, 8, ColorType.TrueColorWithAlpha, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_Grayscale_16bit()
    {
        var pixels = CreateTestPixels(4, 4, 2); // 1 channel * 2 bytes
        var png = CreateAdam7Png(4, 4, ColorType.Grayscale, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    // --- Sub-byte bit depth + Adam7 ---

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void RoundTrip_Adam7_Grayscale_SubByte(byte bitDepth)
    {
        var maxVal = (1 << bitDepth) - 1;
        var pixels = new byte[8 * 8];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % (maxVal + 1));

        var png = CreateAdam7Png(8, 8, ColorType.Grayscale, bitDepth, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_IndexedColor_4bit()
    {
        var plte = new PlteChunkData { Entries = new byte[16 * 3] };
        var pixels = new byte[8 * 8];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 16);

        var png = CreateAdam7IndexedPng(8, 8, 4, plte, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    // --- Small images (empty pass handling) ---

    [Fact]
    public void RoundTrip_Adam7_1x1()
    {
        byte[] pixels = [42, 100, 200, 255];
        var png = CreateAdam7Png(1, 1, ColorType.TrueColorWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_2x2()
    {
        var pixels = CreateTestPixels(2, 2, 4);
        var png = CreateAdam7Png(2, 2, ColorType.TrueColorWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_3x3()
    {
        var pixels = CreateTestPixels(3, 3, 4);
        var png = CreateAdam7Png(3, 3, ColorType.TrueColorWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    // --- Non-power-of-two sizes ---

    [Theory]
    [InlineData(7, 7)]
    [InlineData(9, 5)]
    [InlineData(13, 11)]
    public void RoundTrip_Adam7_NonAlignedSize_Rgba(int width, int height)
    {
        var pixels = CreateTestPixels(width, height, 4);
        var png = CreateAdam7Png(width, height, ColorType.TrueColorWithAlpha, 8, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_Adam7_NonAligned_1bit()
    {
        // 7x7, 1-bit grayscale — exercises sub-byte padding per pass
        var pixels = new byte[7 * 7];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 2);

        var png = CreateAdam7Png(7, 7, ColorType.Grayscale, 1, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    // --- Helpers ---

    private static IRawPng RoundTrip(IRawPng png)
    {
        return Png.DecodeFromByteArray(Png.EncodeToByteArray(png));
    }

    private static byte[] CreateTestPixels(int width, int height, int bytesPerPixel)
    {
        var pixels = new byte[width * height * bytesPerPixel];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 256);
        return pixels;
    }

    private static IRawPng CreateAdam7Png(int width, int height, ColorType colorType, byte bitDepth, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width,
            Height = (uint)height,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.Adam7,
        };
        return Png.Builder().WithIhdr(ihdr).WithPixelData(pixels).Build();
    }

    private static IRawPng CreateAdam7IndexedPng(int width, int height, byte bitDepth, PlteChunkData plte, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width,
            Height = (uint)height,
            BitDepth = bitDepth,
            ColorType = ColorType.IndexedColor,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.Adam7,
        };
        return Png.Builder().WithIhdr(ihdr).WithPlte(plte).WithPixelData(pixels).Build();
    }
}
