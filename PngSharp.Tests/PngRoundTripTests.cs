using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using Xunit;

namespace PngSharp.Tests;

public class PngRoundTripTests
{
    private static IRawPng RoundTrip(IRawPng png)
    {
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var bytes = ms.ToArray();
        return Png.DecodeFromByteArray(bytes);
    }

    [Fact]
    public void RoundTrip_Rgba_PixelDataPreserved()
    {
        byte[] pixels = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 128, 128, 128, 255];
        var png = Png.CreateRgba(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_Rgb_PixelDataPreserved()
    {
        byte[] pixels = [255, 0, 0, 0, 255, 0, 0, 0, 255, 128, 128, 128];
        var png = Png.CreateRgb(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_Grayscale_PixelDataPreserved()
    {
        byte[] pixels = [0, 85, 170, 255];
        var png = Png.CreateGrayscale(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_GrayscaleWithAlpha_PixelDataPreserved()
    {
        byte[] pixels = [0, 255, 85, 200, 170, 128, 255, 50];
        var png = Png.CreateGrayscaleWithAlpha(2, 2, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(png.Ihdr.ColorType, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void Encode_ProducesValidPngStructure()
    {
        // RFC 2083 §3.1: PNG signature is 8 bytes: 137 80 78 71 13 10 26 10
        // RFC 2083 §4.1.1: First chunk must be IHDR with 13 bytes of data
        var png = Png.CreateRgba(2, 2, new byte[16]);
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var bytes = ms.ToArray();

        // PNG signature
        byte[] expectedSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(expectedSignature, bytes[..8]);

        // IHDR chunk: length = 13 (big-endian), type = "IHDR"
        Assert.Equal(0x00, bytes[8]);
        Assert.Equal(0x00, bytes[9]);
        Assert.Equal(0x00, bytes[10]);
        Assert.Equal(0x0D, bytes[11]);
        Assert.Equal((byte)'I', bytes[12]);
        Assert.Equal((byte)'H', bytes[13]);
        Assert.Equal((byte)'D', bytes[14]);
        Assert.Equal((byte)'R', bytes[15]);

        // IHDR data: width=2, height=2 (big-endian uint32)
        Assert.Equal(0x00, bytes[16]);
        Assert.Equal(0x00, bytes[17]);
        Assert.Equal(0x00, bytes[18]);
        Assert.Equal(0x02, bytes[19]);
        Assert.Equal(0x00, bytes[20]);
        Assert.Equal(0x00, bytes[21]);
        Assert.Equal(0x00, bytes[22]);
        Assert.Equal(0x02, bytes[23]);

        // Bit depth=8, color type=6 (RGBA)
        Assert.Equal(8, bytes[24]);
        Assert.Equal(6, bytes[25]);

        // Compression method=0, filter method=0, interlace method=0
        Assert.Equal((byte)CompressionMethod.DeflateWithSlidingWindow, bytes[26]);
        Assert.Equal((byte)FilterMethod.AdaptiveFiltering, bytes[27]);
        Assert.Equal((byte)InterlaceMethod.None, bytes[28]);
    }

    [Fact]
    public void Encode_IHDRChunk_WritesInterlaceMethod()
    {
        var ihdr = new IhdrChunkData
        {
            Width = 2,
            Height = 2,
            BitDepth = 8,
            ColorType = ColorType.TrueColorWithAlpha,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.Adam7,
        };
        var png = Png.Builder()
            .WithIhdr(ihdr)
            .WithPixelData(new byte[16])
            .Build();

        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        var bytes = ms.ToArray();

        // Byte 28 is the interlace method in the IHDR chunk
        Assert.Equal((byte)InterlaceMethod.Adam7, bytes[28]);
    }

    [Fact]
    public void Decode_Test2x2Asset_Succeeds()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "test_2x2.png");
        var bytes = File.ReadAllBytes(path);
        var png = Png.DecodeFromByteArray(bytes);

        Assert.Equal(2u, png.Ihdr.Width);
        Assert.Equal(2u, png.Ihdr.Height);
        Assert.NotNull(png.PixelData);
        var expectedLength = 2 * 2 * png.Ihdr.GetBytesPerPixel();
        Assert.Equal(expectedLength, png.PixelData.Length);
    }

    [Theory]
    [InlineData("test_2x2.png")]
    [InlineData("test_4x4.png")]
    [InlineData("test_64x64.png")]
    [InlineData("diamond_helm.png")]
    [InlineData("diamond_helm_extra_small.png")]
    [InlineData("diamond_helm_grayscale.png")]
    [InlineData("sprite_atlas.png")]
    [InlineData("sprite_atlas_128x64.png")]
    public void RoundTrip_Asset_PixelDataPreserved(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        var bytes = File.ReadAllBytes(path);
        var decoded = Png.DecodeFromByteArray(bytes);

        var reEncoded = RoundTrip(decoded);

        Assert.Equal(decoded.Ihdr.Width, reEncoded.Ihdr.Width);
        Assert.Equal(decoded.Ihdr.Height, reEncoded.Ihdr.Height);
        Assert.Equal(decoded.Ihdr.ColorType, reEncoded.Ihdr.ColorType);
        Assert.Equal(decoded.Ihdr.BitDepth, reEncoded.Ihdr.BitDepth);
        Assert.Equal(decoded.PixelData, reEncoded.PixelData);
    }

    // --- 16-bit round-trip tests ---

    [Fact]
    public void RoundTrip_Grayscale_16Bit_PixelDataPreserved()
    {
        // 2x2, 2 bytes per pixel (big-endian 16-bit samples)
        byte[] pixels = [0x00, 0xFF, 0x80, 0x00, 0xFF, 0xFF, 0x00, 0x00];
        var png = CreatePng(2, 2, ColorType.Grayscale, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.Grayscale, decoded.Ihdr.ColorType);
        Assert.Equal(16, decoded.Ihdr.BitDepth);
    }

    [Fact]
    public void RoundTrip_TrueColor_16Bit_PixelDataPreserved()
    {
        // 2x1, 6 bytes per pixel (R16 G16 B16)
        byte[] pixels = [0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0x80, 0x00, 0xFF, 0x00];
        var png = CreatePng(2, 1, ColorType.TrueColor, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.TrueColor, decoded.Ihdr.ColorType);
        Assert.Equal(16, decoded.Ihdr.BitDepth);
    }

    [Fact]
    public void RoundTrip_TrueColorWithAlpha_16Bit_PixelDataPreserved()
    {
        // 2x1, 8 bytes per pixel (R16 G16 B16 A16)
        byte[] pixels =
        [
            0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0xFF, 0xFF,
            0x00, 0xFF, 0x80, 0x00, 0xFF, 0x00, 0x80, 0x80,
        ];
        var png = CreatePng(2, 1, ColorType.TrueColorWithAlpha, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.TrueColorWithAlpha, decoded.Ihdr.ColorType);
        Assert.Equal(16, decoded.Ihdr.BitDepth);
    }

    [Fact]
    public void RoundTrip_GrayscaleWithAlpha_16Bit_PixelDataPreserved()
    {
        // 2x2, 4 bytes per pixel (G16 A16)
        byte[] pixels =
        [
            0x00, 0x00, 0xFF, 0xFF,
            0x80, 0x00, 0x80, 0x00,
            0xFF, 0xFF, 0x00, 0x00,
            0x40, 0x40, 0xC0, 0xC0,
        ];
        var png = CreatePng(2, 2, ColorType.GrayscaleWithAlpha, 16, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.GrayscaleWithAlpha, decoded.Ihdr.ColorType);
        Assert.Equal(16, decoded.Ihdr.BitDepth);
    }

    // --- Sub-byte round-trip tests ---

    [Theory]
    [InlineData(1, new byte[] { 0, 1, 1, 0 })]
    [InlineData(2, new byte[] { 0, 1, 2, 3 })]
    [InlineData(4, new byte[] { 0, 5, 10, 15 })]
    public void RoundTrip_Grayscale_SubByteBitDepth_PixelDataPreserved(byte bitDepth, byte[] pixels)
    {
        var png = CreatePng(2, 2, ColorType.Grayscale, bitDepth, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.Grayscale, decoded.Ihdr.ColorType);
        Assert.Equal(bitDepth, decoded.Ihdr.BitDepth);
    }

    [Fact]
    public void RoundTrip_Grayscale_1Bit_NonAlignedWidth_PixelDataPreserved()
    {
        // 7 pixels wide: 7 bits -> 1 byte per scanline, with 1 padding bit
        byte[] pixels = [1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0];
        var png = CreatePng(7, 2, ColorType.Grayscale, 1, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
    }

    [Fact]
    public void RoundTrip_IndexedColor_4Bit_PixelDataPreserved()
    {
        // 16-entry palette for 4-bit
        var plte = new PlteChunkData { Entries = new byte[16 * 3] };
        byte[] pixels = [0, 3, 7, 15, 1, 8, 12, 14];
        var png = CreateIndexedPng(4, 2, 4, plte, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.IndexedColor, decoded.Ihdr.ColorType);
    }

    [Fact]
    public void RoundTrip_IndexedColor_1Bit_PixelDataPreserved()
    {
        // 2-entry palette for 1-bit
        var plte = new PlteChunkData { Entries = [0, 0, 0, 255, 255, 255] };
        byte[] pixels = [1, 0, 1, 0, 1, 0, 1, 0];
        var png = CreateIndexedPng(8, 1, 1, plte, pixels);
        var decoded = RoundTrip(png);

        Assert.Equal(pixels, decoded.PixelData);
        Assert.Equal(ColorType.IndexedColor, decoded.Ihdr.ColorType);
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

    private static IRawPng CreatePng(int width, int height, ColorType colorType, byte bitDepth, byte[] pixels)
    {
        var ihdr = new IhdrChunkData
        {
            Width = (uint)width,
            Height = (uint)height,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
        return Png.Builder().WithIhdr(ihdr).WithPixelData(pixels).Build();
    }
}
