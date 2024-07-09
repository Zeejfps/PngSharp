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
        return ColorType switch
        {
            ColorType.Grayscale => 1,
            ColorType.TrueColor => 3,
            ColorType.IndexedColor => 1,
            ColorType.GrayscaleWithAlpha => 2,
            ColorType.TrueColorWithAlpha => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
        
    public override string ToString()
    {
        return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(BitDepth)}: {BitDepth}, {nameof(ColorType)}: {ColorType}, {nameof(CompressionMethod)}: {CompressionMethod}, {nameof(FilterMethod)}: {FilterMethod}, {nameof(InterlaceMethod)}: {InterlaceMethod}";
    }
}