using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Api;

public interface IRawPng
{
    IhdrChunkData Ihdr { get; }

    /// <summary>
    /// Order of bytes is determined by <seealso cref="IhdrChunkData.ColorType"/>.
    /// Ex: RGBA format stores the pixels in [R,G,B,A] where each byte represents a color channel
    /// </summary>
    byte[] PixelData { get; }

    SrgbChunkData? Srgb { get; }
    GammaChunkData? Gama { get; }
    PhysChunkData? Phys { get; }
}