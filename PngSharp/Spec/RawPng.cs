using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Spec;

internal sealed class RawPng : IRawPng
{
    public int Width { get; init; }
    public int Height { get; init; }
    public ColorType ColorType { get; init; }
    public int BytesPerPixel { get; init; }
    public required byte[] PixelData { get; init; }
    public SrgbChunkData? Srgb { get; init; }
    public GammaChunkData? Gama { get; init; }
    public PhysChunkData? Phys { get; init; }
}
