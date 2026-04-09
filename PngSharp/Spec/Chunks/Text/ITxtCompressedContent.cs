namespace PngSharp.Spec.Chunks.Text;

public readonly record struct ITxtCompressedContent
{
    public required string Keyword { get; init; }
    public required string Text { get; init; }
    public required string LanguageTag { get; init; }
    public required string TranslatedKeyword { get; init; }
}
