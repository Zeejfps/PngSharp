using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Api;

public interface IRawPngBuilder
{
    IRawPngBuilder WithIhdr(IhdrChunkData ihdr);
    IRawPngBuilder WithPixelData(byte[] pixels);
    IRawPngBuilder WithSrgb(SrgbChunkData srgb);
    IRawPngBuilder WithGama(GammaChunkData gama);
    IRawPngBuilder WithPhys(PhysChunkData phys);
    IRawPng Build();
}
