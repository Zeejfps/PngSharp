namespace PngSharp.Spec.Chunks.cHRM;

public readonly record struct ChrmChunkData
{
    public uint WhitePointX { get; init; }
    public uint WhitePointY { get; init; }
    public uint RedX { get; init; }
    public uint RedY { get; init; }
    public uint GreenX { get; init; }
    public uint GreenY { get; init; }
    public uint BlueX { get; init; }
    public uint BlueY { get; init; }
}
