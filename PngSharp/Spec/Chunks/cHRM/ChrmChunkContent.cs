namespace PngSharp.Spec.Chunks.cHRM;

public readonly record struct ChrmChunkContent
{
    public double WhitePointX { get; init; }
    public double WhitePointY { get; init; }
    public double RedX { get; init; }
    public double RedY { get; init; }
    public double GreenX { get; init; }
    public double GreenY { get; init; }
    public double BlueX { get; init; }
    public double BlueY { get; init; }
}
