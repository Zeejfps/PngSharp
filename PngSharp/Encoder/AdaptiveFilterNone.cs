namespace PngSharp.Encoder;

public sealed class AdaptiveFilterNone : AdaptiveFilter
{
    public AdaptiveFilterNone(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        return currentRowBuffer[currentIndex];
    }
}