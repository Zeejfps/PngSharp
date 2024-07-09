using PngSharp.Api;
using PngSharp.Common;

namespace PngSharp.Decoder;

internal sealed class DecodedPng : IDecodedPng
{
    public int Width { get; init; }
    public int Height { get; init; }
    public PngSpec.ColorType ColorType { get; init; }
    public byte[] PixelData { get; init; }
    public int BytesPerPixel { get; init; }
    
    public AncillaryChunk<PngSpec.SrgbChunkData> Srgb { get; }
    public AncillaryChunk<PngSpec.GammaChunkData> Gama { get; }
}