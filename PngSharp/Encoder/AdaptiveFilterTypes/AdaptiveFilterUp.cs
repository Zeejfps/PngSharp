namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal sealed class AdaptiveFilterUp : AdaptiveFilter
{
    public AdaptiveFilterUp(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterType Type => PngSpec.AdaptiveFilterType.Up;

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - above);
    }
}