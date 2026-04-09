using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Spec;

internal sealed class RawPngBuilder : IRawPngBuilder
{
    private IhdrChunkData? m_Ihdr;
    private byte[]? m_PixelData;
    private SrgbChunkData? m_Srgb;
    private GammaChunkData? m_Gama;
    private PhysChunkData? m_Phys;

    public IRawPngBuilder WithIhdr(IhdrChunkData ihdr)
    {
        m_Ihdr = ihdr;
        return this;
    }

    public IRawPngBuilder WithPixelData(byte[] pixels)
    {
        m_PixelData = pixels;
        return this;
    }

    public IRawPngBuilder WithSrgb(SrgbChunkData srgb)
    {
        m_Srgb = srgb;
        return this;
    }

    public IRawPngBuilder WithGama(GammaChunkData gama)
    {
        m_Gama = gama;
        return this;
    }

    public IRawPngBuilder WithPhys(PhysChunkData phys)
    {
        m_Phys = phys;
        return this;
    }

    public IRawPng Build()
    {
        if (m_Ihdr is null)
            throw new InvalidOperationException("Ihdr is required.");
        if (m_PixelData is null)
            throw new InvalidOperationException("PixelData is required.");

        var ihdr = m_Ihdr.Value;

        if (ihdr.Width == 0)
            throw new InvalidOperationException("Width must be greater than zero.");
        if (ihdr.Height == 0)
            throw new InvalidOperationException("Height must be greater than zero.");

        ValidateBitDepth(ihdr.BitDepth, ihdr.ColorType);

        var expectedLength = (int)ihdr.Width * (int)ihdr.Height * ihdr.GetBytesPerPixel();
        if (m_PixelData.Length != expectedLength)
            throw new InvalidOperationException(
                $"PixelData length {m_PixelData.Length} does not match expected length {expectedLength} " +
                $"for a {ihdr.Width}x{ihdr.Height} image with {ihdr.ColorType} color type and {ihdr.BitDepth}-bit depth.");

        return new RawPng
        {
            Ihdr = ihdr,
            PixelData = m_PixelData,
            Srgb = m_Srgb,
            Gama = m_Gama,
            Phys = m_Phys,
        };
    }

    private static void ValidateBitDepth(byte bitDepth, ColorType colorType)
    {
        byte[] allowed = colorType switch
        {
            ColorType.Grayscale => [1, 2, 4, 8, 16],
            ColorType.TrueColor => [8, 16],
            ColorType.IndexedColor => [1, 2, 4, 8],
            ColorType.GrayscaleWithAlpha => [8, 16],
            ColorType.TrueColorWithAlpha => [8, 16],
            _ => throw new InvalidOperationException($"Unknown ColorType: {colorType}."),
        };

        if (Array.IndexOf(allowed, bitDepth) < 0)
            throw new InvalidOperationException(
                $"BitDepth {bitDepth} is not valid for {colorType}. Allowed values: {string.Join(", ", allowed)}.");
    }
}
