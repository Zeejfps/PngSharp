namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal sealed class AdaptiveFilterTypeNone : AdaptiveFilterTypeBase
{
    public AdaptiveFilterTypeNone(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.None;

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        return currentRowBuffer[currentIndex];
    }
}