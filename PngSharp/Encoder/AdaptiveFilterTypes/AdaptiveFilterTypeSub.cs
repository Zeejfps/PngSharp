namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal sealed class AdaptiveFilterTypeSub : AdaptiveFilterTypeBase
{
    public AdaptiveFilterTypeSub(int bytesPerPixel) : base(bytesPerPixel) { }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Sub;

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - left);
    }
}