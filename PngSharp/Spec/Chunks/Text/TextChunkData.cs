namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// tEXt chunk: uncompressed Latin-1 text metadata.
/// </summary>
public readonly record struct TextChunkData
{
    public string Keyword { get; init; }
    public string Text { get; init; }
}
