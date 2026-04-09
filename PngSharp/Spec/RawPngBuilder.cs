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

        return new RawPng
        {
            Ihdr = m_Ihdr.Value,
            PixelData = m_PixelData,
            Srgb = m_Srgb,
            Gama = m_Gama,
            Phys = m_Phys,
        };
    }
}
