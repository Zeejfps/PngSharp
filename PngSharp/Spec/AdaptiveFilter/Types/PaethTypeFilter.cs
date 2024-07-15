namespace PngSharp.Spec.AdaptiveFilter.Types;

internal class PaethTypeFilter : FilterBase
{
    public PaethTypeFilter(int bytesPerPixel) : base(bytesPerPixel) { }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Paeth;
    
    public override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var x = currentRow[currentIndex];
        var left = GetLeftValue(currentRow, currentIndex);
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var aboveLeft = GetAboveLeftByteValue(previousRowBuffer, currentIndex);
        return (byte)(x - PaethPredictor(left, above, aboveLeft));
    }

    public override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var x = currentRow[currByteIndex];
        var left = GetLeftValue(currentRow, currByteIndex);
        var above = GetAboveValue(prevRow, currByteIndex);
        var aboveLeft = GetAboveLeftByteValue(prevRow, currByteIndex);
        return (byte)(x + PaethPredictor(left, above, aboveLeft));
    }

    private byte PaethPredictor(byte left, byte above, byte aboveLeft)
    {
        var p = left + above - aboveLeft;
        var pa = Math.Abs(p - left);
        var pb = Math.Abs(p - above);
        var pc = Math.Abs(p - aboveLeft);

        if (pa <= pb && pa <= pc)
            return left;
        
        if (pb <= pc)
            return above;
        
        return aboveLeft;
    }
}