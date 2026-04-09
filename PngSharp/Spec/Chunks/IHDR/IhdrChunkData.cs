namespace PngSharp.Spec.Chunks.IHDR;

public readonly struct IhdrChunkData
{
    public uint Width { get; init; }
    public uint Height { get; init; }
    public byte BitDepth { get; init; }
    public ColorType ColorType { get; init; }
    public CompressionMethod CompressionMethod { get; init; }
    public FilterMethod FilterMethod { get; init; }
    public InterlaceMethod InterlaceMethod { get; init; }


    public int GetBytesPerPixel()
    {
        var bitsPerPixel = ColorType switch
        {
            ColorType.Grayscale => BitDepth,
            ColorType.TrueColor => BitDepth * 3,
            ColorType.IndexedColor => BitDepth,
            ColorType.GrayscaleWithAlpha => BitDepth * 2,
            ColorType.TrueColorWithAlpha => BitDepth * 4,
            _ => throw new ArgumentOutOfRangeException()
        };
        return (bitsPerPixel + 7) / 8;
    }
        
    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(BitDepth)}: {BitDepth}, {nameof(ColorType)}: {ColorType}, {nameof(CompressionMethod)}: {CompressionMethod}, {nameof(FilterMethod)}: {FilterMethod}, {nameof(InterlaceMethod)}: {InterlaceMethod}";
    }
}