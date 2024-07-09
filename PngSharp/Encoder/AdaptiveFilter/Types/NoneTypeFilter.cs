namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal sealed class NoneTypeFilter : FilterBase
{
    public NoneTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.None;

    protected override byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        return currentRowBuffer[currentIndex];
    }
}