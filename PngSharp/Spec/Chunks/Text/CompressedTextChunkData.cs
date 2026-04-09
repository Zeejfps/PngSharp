namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// zTXt chunk: deflate-compressed Latin-1 text metadata.
/// CompressedData contains the raw deflate bytes. Use TextChunkUtils to decompress.
/// </summary>
public readonly record struct CompressedTextChunkData
{
    public string Keyword { get; init; }
    public byte[] CompressedData { get; init; }
}
