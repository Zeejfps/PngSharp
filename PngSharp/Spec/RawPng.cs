using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Spec;

internal sealed class RawPng : IRawPng
{
    public required IhdrChunkData Ihdr { get; init; }
    public required byte[] PixelData { get; init; }
    public SrgbChunkData? Srgb { get; init; }
    public GammaChunkData? Gama { get; init; }
    public PhysChunkData? Phys { get; init; }
}
