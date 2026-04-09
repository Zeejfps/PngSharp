namespace PngSharp.Spec.Chunks.tIME;

public readonly record struct TimeChunkData
{
    public ushort Year { get; init; }
    public byte Month { get; init; }
    public byte Day { get; init; }
    public byte Hour { get; init; }
    public byte Minute { get; init; }
    public byte Second { get; init; }
}
