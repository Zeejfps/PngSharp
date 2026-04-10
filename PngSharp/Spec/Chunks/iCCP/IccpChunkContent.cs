namespace PngSharp.Spec.Chunks.iCCP;

public readonly record struct IccpChunkContent
{
    public string ProfileName { get; init; }
    public byte[] RawProfile { get; init; }
}
