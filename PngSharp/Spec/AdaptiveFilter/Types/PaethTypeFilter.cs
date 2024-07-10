namespace PngSharp.Spec.AdaptiveFilter.Types;

internal class PaethTypeFilter : FilterBase
{
    public PaethTypeFilter(int bytesPerPixel) : base(bytesPerPixel) { }

    public override AdaptiveFilterTypeKind Kind => AdaptiveFilterTypeKind.Paeth;
    protected override byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var x = currentRow[currentIndex];
        var left = GetLeftValue(currentRow, currentIndex);
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var aboveLeft = GetAboveLeftByteValue(previousRowBuffer, currentIndex);
        //return (byte)(x - PaethPredictor(a, b, c));

        int p = left + above - aboveLeft;
        int pa = Math.Abs(p - left);
        int pb = Math.Abs(p - above);
        int pc = Math.Abs(p - aboveLeft);
        
        if (pa <= pb && pa <= pc)
            return (byte)((x - left) & 0xFF);
        if (pb <= pc)
            return (byte)((x - above) & 0xFF);
        
        return (byte)((x - aboveLeft) & 0xFF);
    }

    protected override byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        var x = currentRow[currByteIndex];
        var a = GetLeftValue(currentRow, currByteIndex);
        var b = GetAboveValue(prevRow, currByteIndex);
        var c = GetAboveLeftByteValue(prevRow, currByteIndex);
        return (byte)(x + PaethPredictor(a, b, c));
    }

    private byte PaethPredictor(byte left, byte up, byte upLeft)
    {
        var p = left + up - upLeft;
        var pa = Math.Abs(p - left);
        var pb = Math.Abs(p - up);
        var pc = Math.Abs(p - upLeft);

        if (pa <= pb && pa <= pc)
            return left;
        
        if (pb <= pc)
            return up;
        
        return upLeft;
    }
}