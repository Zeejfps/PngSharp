namespace PngSharp.Spec.AdaptiveFilter.Types;

internal sealed class UpTypeFilter : FilterBase
{
    public UpTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Up;

    public override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var above = GetAboveValue(prevRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x - above);
    }

    public override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var above = GetAboveValue(prevRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x + above);
    }
    
}