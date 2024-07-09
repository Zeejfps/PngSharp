namespace PngSharp.Common.AdaptiveFilter.Types;

internal sealed class NoneTypeFilter : FilterBase
{
    public NoneTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.None;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        return currentRow[currentIndex];
    }

    protected override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currentByteIndex)
    {
        return currentRow[currentByteIndex];
    }
}