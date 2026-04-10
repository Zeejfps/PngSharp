using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.tIME;
using PngSharp.Spec.Chunks.tRNS;
using PngSharp.Spec.Chunks.sBIT;
using PngSharp.Spec.Chunks.iCCP;
using PngSharp.Spec.Chunks.eXIf;

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
    ChrmChunkData? Chrm { get; }
    TimeChunkData? Time { get; }
    BkgdChunkData? Bkgd { get; }
    SbitChunkData? Sbit { get; }
    IccpChunkData? Iccp { get; }
    ExifChunkData? Exif { get; }
    IReadOnlyList<TextChunk> TxtChunks { get; }
    IReadOnlyList<ZTextChunk> ZTxtChunks { get; }
    IReadOnlyList<ITextChunk> ITxtChunks { get; }
}