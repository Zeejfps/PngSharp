namespace PngSharp.Spec;

public readonly struct ChunkHeader
{
    public int ChunkSizeInBytes { get; init; }
    public string Name { get; init; }

    public override string ToString()
    {
        return $"{nameof(ChunkSizeInBytes)}: {ChunkSizeInBytes}, {nameof(Name)}: {Name}";
    }
}