using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.tRNS;

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
    public required IReadOnlyList<TxtChunkData> TxtChunks { get; init; }
    public required IReadOnlyList<ZTxtChunkData> ZTxtChunks { get; init; }
    public required IReadOnlyList<ITxtChunkData> ITxtChunks { get; init; }
}
