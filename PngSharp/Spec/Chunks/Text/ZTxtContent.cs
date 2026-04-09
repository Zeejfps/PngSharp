namespace PngSharp.Spec.Chunks.Text;

public readonly record struct ZTxtContent
{
    public string Keyword { get; init; }
    public string Text { get; init; }
}