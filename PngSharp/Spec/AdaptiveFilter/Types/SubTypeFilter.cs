namespace PngSharp.Spec.AdaptiveFilter.Types;

internal sealed class SubTypeFilter : FilterBase
{
    public SubTypeFilter(int bytesPerPixel) : base(bytesPerPixel) { }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Sub;

    public override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> previousRowBuffer, int currByteIndex)
    {
        var left = GetLeftValue(currentRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x - left);
    }

    public override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var left = GetLeftValue(currentRow, currByteIndex);
        var x = currentRow[currByteIndex];
        return (byte)(x + left);
    }
}