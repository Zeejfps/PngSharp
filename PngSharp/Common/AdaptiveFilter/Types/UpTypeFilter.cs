using PngSharp.Spec;

namespace PngSharp.Common.AdaptiveFilter.Types;

internal sealed class UpTypeFilter : FilterBase
{
    public UpTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Up;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var above = GetAboveValue(prevRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x - above);
    }

    protected override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var above = GetAboveValue(prevRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x + above);
    }
    
}