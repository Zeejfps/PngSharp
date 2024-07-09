namespace PngSharp.Encoder.AdaptiveFilter.Types;

public class AverageTypeFilter : FilterBase
{
    public AverageTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Average;
    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var x = currentRowBuffer[currentIndex];
        return (byte)(x - ((left + above) / 2));
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