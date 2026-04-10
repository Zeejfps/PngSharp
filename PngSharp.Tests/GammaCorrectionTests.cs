using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using Xunit;
using static PngSharp.Tests.PngTestHelpers;

namespace PngSharp.Tests;

public class GammaCorrectionTests
{
    // --- GetFileGamma ---

    [Fact]
    public void GetFileGamma_WithSrgbChunk_ReturnsApprox0_45455()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var gamma = png.GetFileGamma();

        Assert.NotNull(gamma);
        Assert.Equal(1.0 / 2.2, gamma!.Value, precision: 5);
    }

    [Fact]
    public void GetFileGamma_WithGamaChunk_ReturnsGamaValue()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        var gamma = png.GetFileGamma();

        Assert.NotNull(gamma);
        Assert.Equal(0.45455, gamma!.Value, precision: 5);
    }

    [Fact]
    public void GetFileGamma_WithGamaChunk_LinearGamma()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithGama(GammaChunkData.FromDouble(1.0))
            .Build();

        var gamma = png.GetFileGamma();

        Assert.NotNull(gamma);
        Assert.Equal(1.0, gamma!.Value, precision: 5);
    }

    [Fact]
    public void GetFileGamma_SrgbTakesPrecedenceOverGama()
    {
        // sRGB + gAMA is allowed (spec says encoders should include gAMA for compatibility)
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .WithGama(GammaChunkData.FromDouble(1.8)) // different value
            .Build();

        var gamma = png.GetFileGamma();

        Assert.NotNull(gamma);
        Assert.Equal(1.0 / 2.2, gamma!.Value, precision: 5);
    }

    [Fact]
    public void GetFileGamma_NoGammaChunks_ReturnsNull()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        Assert.Null(png.GetFileGamma());
    }

    // --- ApplyGammaCorrection ---

    [Fact]
    public void ApplyGammaCorrection_IdentityGamma_NoChange()
    {
        // fileGamma (1/2.2) * displayGamma (2.2) = 1.0 -> exponent = 1.0 -> identity
        byte[] pixels = [100, 150, 200, 50, 75, 25, 200, 100, 50, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(new GammaChunkData { Value = 45455 }) // 1/2.2
            .Build();

        var result = png.ApplyGammaCorrection(2.2);

        Assert.Equal(pixels, result);
    }

    [Fact]
    public void ApplyGammaCorrection_TrueColorWithAlpha_AlphaPreserved()
    {
        byte[] pixels = [128, 64, 32, 200, 64, 128, 32, 100, 255, 0, 128, 50, 0, 255, 64, 80];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColorWithAlpha))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(0.5))
            .Build();

        var result = png.ApplyGammaCorrection(1.0);

        // Alpha bytes (index 3, 7, 11, 15) must be unchanged
        Assert.Equal(200, result[3]);
        Assert.Equal(100, result[7]);
        Assert.Equal(50, result[11]);
        Assert.Equal(80, result[15]);
    }

    [Fact]
    public void ApplyGammaCorrection_Grayscale_CorrectValues()
    {
        byte[] pixels = [0, 128, 255, 64];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.Grayscale))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(0.5))
            .Build();

        var result = png.ApplyGammaCorrection(1.0);

        // exponent = 1/(0.5*1.0) = 2.0
        // 0^2 = 0, 255^2/255 = 255 (fixed points)
        Assert.Equal(0, result[0]);
        Assert.Equal(255, result[2]);
        // 128/255 = 0.502 -> 0.502^2 = 0.252 -> 0.252*255 = 64
        Assert.Equal(64, result[1]);
    }

    [Fact]
    public void ApplyGammaCorrection_GrayscaleWithAlpha_AlphaPreserved()
    {
        byte[] pixels = [128, 200, 64, 150, 255, 100, 0, 50];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.GrayscaleWithAlpha))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(0.5))
            .Build();

        var result = png.ApplyGammaCorrection(1.0);

        // Alpha at indices 1, 3, 5, 7 must be unchanged
        Assert.Equal(200, result[1]);
        Assert.Equal(150, result[3]);
        Assert.Equal(100, result[5]);
        Assert.Equal(50, result[7]);
    }

    [Fact]
    public void ApplyGammaCorrection_NoGammaInfo_Throws()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        Assert.Throws<InvalidOperationException>(() => png.ApplyGammaCorrection());
    }

    [Fact]
    public void ApplyGammaCorrection_ZeroDisplayGamma_Throws()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => png.ApplyGammaCorrection(0));
    }

    [Fact]
    public void ApplyGammaCorrection_NegativeDisplayGamma_Throws()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => png.ApplyGammaCorrection(-1.0));
    }

    [Fact]
    public void ApplyGammaCorrection_ReturnsNewArray()
    {
        byte[] pixels = [100, 150, 200, 50, 75, 25, 200, 100, 50, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        var result = png.ApplyGammaCorrection();

        Assert.NotSame(png.PixelData, result);
    }

    [Fact]
    public void ApplyGammaCorrection_BlackAndWhite_AlwaysFixedPoints()
    {
        byte[] pixels = [0, 0, 0, 255, 255, 255, 0, 0, 0, 255, 255, 255];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(0.3))
            .Build();

        var result = png.ApplyGammaCorrection(1.5);

        Assert.Equal(0, result[0]);
        Assert.Equal(0, result[1]);
        Assert.Equal(0, result[2]);
        Assert.Equal(255, result[3]);
        Assert.Equal(255, result[4]);
        Assert.Equal(255, result[5]);
    }

    [Fact]
    public void ApplyGammaCorrection_SinglePixel()
    {
        byte[] pixels = [128, 64, 32, 255];
        var png = Png.Builder()
            .WithIhdr(new IhdrChunkData
            {
                Width = 1, Height = 1, BitDepth = 8,
                ColorType = ColorType.TrueColorWithAlpha,
                CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
                FilterMethod = FilterMethod.AdaptiveFiltering,
                InterlaceMethod = InterlaceMethod.None,
            })
            .WithPixelData(pixels)
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        var result = png.ApplyGammaCorrection();

        Assert.Equal(4, result.Length);
        Assert.Equal(255, result[3]); // alpha preserved
    }

    [Fact]
    public void ApplyGammaCorrection_IndexedColor_ExpandsToRgb()
    {
        byte[] paletteEntries = [255, 0, 0, 0, 255, 0]; // red, green
        byte[] pixels = [0, 1, 1, 0]; // indices into palette

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPixelData(pixels)
            .WithPlte(new PlteChunkData { Entries = paletteEntries })
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        var result = png.ApplyGammaCorrection();

        // 4 pixels * 3 bytes/pixel = 12 bytes (expanded from indexed)
        Assert.Equal(12, result.Length);
    }

    // --- ToLinear ---

    [Fact]
    public void ToLinear_WithSrgb_UsesExactTransferFunction()
    {
        // sRGB 188 -> linear ~0.5 -> byte ~128
        byte[] pixels = [188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188, 188];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var result = png.ToLinear();

        // sRGB 188/255 = 0.7373 -> linear = ((0.7373+0.055)/1.055)^2.4 ≈ 0.5028 -> byte ≈ 128
        Assert.InRange(result[0], 126, 130);
    }

    [Fact]
    public void ToLinear_WithGama_UsesPowerLaw()
    {
        byte[] pixels = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(new GammaChunkData { Value = 45455 }) // ~1/2.2
            .Build();

        var result = png.ToLinear();

        // 128/255 = 0.502 -> (0.502)^(1/0.45455) = (0.502)^2.2 ≈ 0.218 -> byte ≈ 56
        Assert.InRange(result[0], 54, 58);
    }

    [Fact]
    public void ToLinear_AlphaPreserved()
    {
        byte[] pixels = [128, 200, 128, 150, 128, 100, 128, 50];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.GrayscaleWithAlpha))
            .WithPixelData(pixels)
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var result = png.ToLinear();

        Assert.Equal(200, result[1]);
        Assert.Equal(150, result[3]);
        Assert.Equal(100, result[5]);
        Assert.Equal(50, result[7]);
    }

    [Fact]
    public void ToLinear_NoGammaInfo_Throws()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        Assert.Throws<InvalidOperationException>(() => png.ToLinear());
    }

    [Fact]
    public void ToLinear_BlackAndWhite_FixedPoints()
    {
        byte[] pixels = [0, 255, 0, 255, 255, 128, 128, 64];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.GrayscaleWithAlpha))
            .WithPixelData(pixels)
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var result = png.ToLinear();

        Assert.Equal(0, result[0]);
        Assert.Equal(255, result[4]);
    }

    // --- ToSrgb ---

    [Fact]
    public void ToSrgb_AlreadySrgb_ReturnsCopy()
    {
        byte[] pixels = [100, 150, 200, 50, 75, 25, 200, 100, 50, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var result = png.ToSrgb();

        Assert.Equal(pixels, result);
        Assert.NotSame(png.PixelData, result);
    }

    [Fact]
    public void ToSrgb_FromLinearGamma_CorrectValues()
    {
        // Linear data (gAMA = 1.0) -> sRGB encoding
        // Linear 128/255 = 0.502 -> sRGB = 1.055*(0.502)^(1/2.4) - 0.055 ≈ 0.735 -> byte ≈ 188
        byte[] pixels = [128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(1.0))
            .Build();

        var result = png.ToSrgb();

        Assert.InRange(result[0], 186, 190);
    }

    [Fact]
    public void ToSrgb_ReturnsNewArray()
    {
        byte[] pixels = [100, 150, 200, 50, 75, 25, 200, 100, 50, 128, 128, 128];
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(pixels)
            .WithGama(GammaChunkData.FromDouble(1.0))
            .Build();

        var result = png.ToSrgb();

        Assert.NotSame(png.PixelData, result);
    }

    [Fact]
    public void ToSrgb_NoGammaInfo_Throws()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .Build();

        Assert.Throws<InvalidOperationException>(() => png.ToSrgb());
    }

    // --- GetGammaCorrectedPalette ---

    [Fact]
    public void GetGammaCorrectedPalette_ReturnsCorrectedEntries()
    {
        byte[] paletteEntries = [0, 0, 0, 255, 255, 255, 128, 64, 32];
        byte[] pixels = [0, 1, 2, 0];

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPixelData(pixels)
            .WithPlte(new PlteChunkData { Entries = paletteEntries })
            .WithGama(GammaChunkData.FromDouble(0.5))
            .Build();

        var result = png.GetGammaCorrectedPalette(1.0);

        Assert.NotNull(result);
        var entries = result!.Value.Entries;
        Assert.Equal(9, entries.Length);
        // Black and white are fixed points
        Assert.Equal(0, entries[0]);
        Assert.Equal(0, entries[1]);
        Assert.Equal(0, entries[2]);
        Assert.Equal(255, entries[3]);
        Assert.Equal(255, entries[4]);
        Assert.Equal(255, entries[5]);
    }

    [Fact]
    public void GetGammaCorrectedPalette_NoPalette_ReturnsNull()
    {
        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.TrueColor))
            .WithPixelData(new byte[2 * 2 * 3])
            .WithGama(new GammaChunkData { Value = 45455 })
            .Build();

        Assert.Null(png.GetGammaCorrectedPalette());
    }

    [Fact]
    public void GetGammaCorrectedPalette_NoGammaInfo_Throws()
    {
        byte[] paletteEntries = [255, 0, 0, 0, 255, 0];
        byte[] pixels = [0, 1, 1, 0];

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPixelData(pixels)
            .WithPlte(new PlteChunkData { Entries = paletteEntries })
            .Build();

        Assert.Throws<InvalidOperationException>(() => png.GetGammaCorrectedPalette());
    }

    // --- ToSrgb with IndexedColor ---

    [Fact]
    public void ToSrgb_IndexedColor_WithSrgb_ExpandsToRgb()
    {
        byte[] paletteEntries = [255, 0, 0, 0, 255, 0];
        byte[] pixels = [0, 1, 1, 0];

        var png = Png.Builder()
            .WithIhdr(CreateIhdr(ColorType.IndexedColor))
            .WithPixelData(pixels)
            .WithPlte(new PlteChunkData { Entries = paletteEntries })
            .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
            .Build();

        var result = png.ToSrgb();

        Assert.Equal(12, result.Length); // 4 pixels * 3 bytes
        // First pixel: palette[0] = red
        Assert.Equal(255, result[0]);
        Assert.Equal(0, result[1]);
        Assert.Equal(0, result[2]);
        // Second pixel: palette[1] = green
        Assert.Equal(0, result[3]);
        Assert.Equal(255, result[4]);
        Assert.Equal(0, result[5]);
    }
}
