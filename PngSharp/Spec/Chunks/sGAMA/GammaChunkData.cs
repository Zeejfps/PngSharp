namespace PngSharp.Spec.Chunks.sGAMA;

public readonly record struct GammaChunkData
{
    public uint Value { get; init; }
}
