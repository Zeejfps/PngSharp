namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// iTXt chunk: international UTF-8 text metadata with language tag.
/// Data contains raw bytes — UTF-8 text if uncompressed, deflate bytes if compressed.
/// Use TextChunkUtils to get the text.
/// </summary>
public readonly record struct InternationalTextChunkData
{
    public string Keyword { get; init; }
    public string LanguageTag { get; init; }
    public string TranslatedKeyword { get; init; }
    public bool IsCompressed { get; init; }
    public byte[] Data { get; init; }
}
