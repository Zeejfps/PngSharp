namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal sealed class SubTypeFilter : FilterBase
{
    public SubTypeFilter(int bytesPerPixel) : base(bytesPerPixel) { }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Sub;

    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currByteIndex)
    {
        var left = GetLeftValue(currentRowBuffer, currByteIndex);
        var x = currentRowBuffer[currByteIndex];
        return (byte)(x - left);
    }

    protected override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var x = currentRow[currByteIndex];
        var leftValue = GetLeftValue(currentRow, currByteIndex);
        return (byte)(x + leftValue);
    }
}