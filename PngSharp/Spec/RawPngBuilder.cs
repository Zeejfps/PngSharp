using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Spec;

internal sealed class RawPngBuilder : IRawPngBuilder
{
    private IhdrChunkData? _ihdr;
    private byte[]? _pixelData;
    private SrgbChunkData? _srgb;
    private GammaChunkData? _gama;
    private PhysChunkData? _phys;

    public IRawPngBuilder WithIhdr(IhdrChunkData ihdr)
    {
        _ihdr = ihdr;
        return this;
    }

    public IRawPngBuilder WithPixelData(byte[] pixels)
    {
        _pixelData = pixels;
        return this;
    }

    public IRawPngBuilder WithSrgb(SrgbChunkData srgb)
    {
        _srgb = srgb;
        return this;
    }

    public IRawPngBuilder WithGama(GammaChunkData gama)
    {
        _gama = gama;
        return this;
    }

    public IRawPngBuilder WithPhys(PhysChunkData phys)
    {
        _phys = phys;
        return this;
    }

    public IRawPng Build()
    {
        if (_ihdr is null)
            throw new InvalidOperationException("Ihdr is required.");
        if (_pixelData is null)
            throw new InvalidOperationException("PixelData is required.");

        return new RawPng
        {
            Ihdr = _ihdr.Value,
            PixelData = _pixelData,
            Srgb = _srgb,
            Gama = _gama,
            Phys = _phys,
        };
    }
}
