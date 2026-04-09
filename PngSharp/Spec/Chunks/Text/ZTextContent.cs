namespace PngSharp.Spec.Chunks.Text;

public readonly record struct ZTextContent
{
    public string Keyword { get; init; }
    public string Text { get; init; }
}