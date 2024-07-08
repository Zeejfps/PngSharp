namespace PngSharp.Encoder.AdaptiveFilterTypes;

public class AdaptiveFilterTypeAverage : AdaptiveFilterTypeBase
{
    public AdaptiveFilterTypeAverage(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Average;
    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var above = GetAboveValue(currentRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - ((left + above) / 2));
    }
}