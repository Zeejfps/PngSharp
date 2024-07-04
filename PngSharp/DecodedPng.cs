namespace PngSharp;

internal sealed class DecodedPng : IDecodedPng
{
    public int Width { get; init; }
    public int Height { get; init; }
    public byte[] PixelData { get; init; }
}