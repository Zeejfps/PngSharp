namespace PngSharp.Encoder;

internal sealed class AdaptiveFilterUp : AdaptiveFilter
{
    public AdaptiveFilterUp(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        var above = GetAboveValue(currentRowBuffer, previousRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - above);
    }
}