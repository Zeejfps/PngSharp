namespace PngSharp.Spec.Chunks.Text;

public readonly record struct TextChunkData
{
    public string Keyword { get; init; }
    public string Text { get; init; }
    public bool IsCompressed { get; init; }
    public string? LanguageTag { get; init; }
    public string? TranslatedKeyword { get; init; }

    /// <summary>
    /// True if this represents an iTXt chunk (international text with language tag).
    /// </summary>
    public bool IsInternational => LanguageTag is not null;
}
