namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal sealed class AdaptiveFilterNone : AdaptiveFilter
{
    public AdaptiveFilterNone(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterType Type => PngSpec.AdaptiveFilterType.None;

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        return currentRowBuffer[currentIndex];
    }
}