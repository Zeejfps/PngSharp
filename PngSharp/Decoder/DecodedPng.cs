namespace PngSharp.Decoder;

internal sealed class DecodedPng : IDecodedPng
{
    public int Width { get; init; }
    public int Height { get; init; }
    public PixelFormat PixelFormat { get; init; }
    public byte[] PixelData { get; init; }
    public int BytesPerPixel { get; init; }
}