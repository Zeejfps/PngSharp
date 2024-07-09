namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal sealed class NoneTypeFilter : FilterBase
{
    public NoneTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.None;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        return currentRowBuffer[currentIndex];
    }
}