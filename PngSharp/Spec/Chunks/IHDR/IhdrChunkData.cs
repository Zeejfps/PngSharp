namespace PngSharp.Spec.Chunks.IHDR;

public readonly record struct IhdrChunkData
{
    public uint Width { get; init; }
    public uint Height { get; init; }
    public byte BitDepth { get; init; }
    public ColorType ColorType { get; init; }
    public CompressionMethod CompressionMethod { get; init; }
    public FilterMethod FilterMethod { get; init; }
    public InterlaceMethod InterlaceMethod { get; init; }

    public int GetBitsPerPixel()
    {
        return ColorType switch
        {
            ColorType.Grayscale => BitDepth,
            ColorType.TrueColor => BitDepth * 3,
            ColorType.IndexedColor => BitDepth,
            ColorType.GrayscaleWithAlpha => BitDepth * 2,
            ColorType.TrueColorWithAlpha => BitDepth * 4,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public int GetBytesPerPixel() => (GetBitsPerPixel() + 7) / 8;

    public int GetScanlineByteWidth() => ((int)Width * GetBitsPerPixel() + 7) / 8;
}
