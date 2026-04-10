using PngSharp.Api;
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

namespace PngSharp.Spec;

internal sealed class RawPng : IRawPng
{
    public required IhdrChunkData Ihdr { get; init; }
    public required byte[] PixelData { get; init; }
    public PlteChunkData? Plte { get; init; }
    public TrnsChunkData? Trns { get; init; }
    public SrgbChunkData? Srgb { get; init; }
    public GammaChunkData? Gama { get; init; }
    public PhysChunkData? Phys { get; init; }
    public ChrmChunkData? Chrm { get; init; }
    public TimeChunkData? Time { get; init; }
    public BkgdChunkData? Bkgd { get; init; }
    public SbitChunkData? Sbit { get; init; }
    public IccpChunkData? Iccp { get; init; }
    public ExifChunkData? Exif { get; init; }
    public required IReadOnlyList<TextChunk> TxtChunks { get; init; }
    public required IReadOnlyList<ZTextChunk> ZTxtChunks { get; init; }
    public required IReadOnlyList<ITextChunk> ITxtChunks { get; init; }
}
