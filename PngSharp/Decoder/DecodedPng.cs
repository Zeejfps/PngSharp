using PngSharp.Api;
using PngSharp.Common;

namespace PngSharp.Decoder;

internal sealed class DecodedPng : IDecodedPng
{
    public int Width { get; set; }
    public int Height { get; set; }
    public PngSpec.ColorType ColorType { get; set; }
    public byte[] PixelData { get; set; }
    public int BytesPerPixel { get; set; }
    public AncillaryChunk<PngSpec.SrgbChunkData> Srgb { get; set; }
    public AncillaryChunk<PngSpec.GammaChunkData> Gama { get; set; }
    public AncillaryChunk<PngSpec.PhysChunkData> Phys { get; set; }
}