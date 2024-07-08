namespace PngSharp.Encoder;

public sealed class AdaptiveFilterSub : AdaptiveFilter
{
    public AdaptiveFilterSub(int bytesPerPixel) : base(bytesPerPixel) { }

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - left);
    }
}