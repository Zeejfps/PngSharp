using PngSharp.Api;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Decoder;

internal sealed class DecodedPng : IDecodedPng
{
    public int Width { get; set; }
    public int Height { get; set; }
    public ColorType ColorType { get; set; }
    public byte[] PixelData { get; set; }
    public int BytesPerPixel { get; set; }
    public AncillaryChunk<SrgbChunkData> Srgb { get; set; }
    public AncillaryChunk<GammaChunkData> Gama { get; set; }
    public AncillaryChunk<PhysChunkData> Phys { get; set; }
}