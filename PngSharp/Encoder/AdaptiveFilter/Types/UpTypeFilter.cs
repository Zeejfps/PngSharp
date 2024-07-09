namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal sealed class UpTypeFilter : FilterBase
{
    public UpTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Up;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - above);
    }
}