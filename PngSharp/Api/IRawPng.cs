using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.tRNS;

namespace PngSharp.Api;

public interface IRawPng
{
    IhdrChunkData Ihdr { get; }

    /// <summary>
    /// Order of bytes is determined by <seealso cref="IhdrChunkData.ColorType"/>.
    /// Ex: RGBA format stores the pixels in [R,G,B,A] where each byte represents a color channel
    /// </summary>
    byte[] PixelData { get; }

    PlteChunkData? Plte { get; }
    TrnsChunkData? Trns { get; }
    SrgbChunkData? Srgb { get; }
    GammaChunkData? Gama { get; }
    PhysChunkData? Phys { get; }
    IReadOnlyList<TextChunkData> TextChunks { get; }
    IReadOnlyList<CompressedTextChunkData> CompressedTextChunks { get; }
    IReadOnlyList<InternationalTextChunkData> InternationalTextChunks { get; }
}