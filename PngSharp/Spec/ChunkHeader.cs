namespace PngSharp.Spec;

internal readonly record struct ChunkHeader
{
    public int ChunkSizeInBytes { get; init; }
    public string Id { get; init; }
}