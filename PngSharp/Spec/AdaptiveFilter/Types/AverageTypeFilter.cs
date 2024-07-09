namespace PngSharp.Spec.AdaptiveFilter.Types;

public class AverageTypeFilter : FilterBase
{
    public AverageTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Average;
    protected override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRow, currentIndex);
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var x = currentRow[currentIndex];
        var reconValue = (left + above) / 2;
        return (byte)(x - reconValue);
    }

    protected override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var x = currentRow[currByteIndex];
        var left = GetLeftValue(currentRow, currByteIndex);
        var above = GetAboveValue(prevRow, currByteIndex);
        var reconValue = (byte)(left + above * 0.5);
        return (byte)(x + reconValue);
    }
}