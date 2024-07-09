namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal sealed class SubTypeFilter : FilterBase
{
    public SubTypeFilter(int bytesPerPixel) : base(bytesPerPixel) { }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Sub;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - left);
    }
}