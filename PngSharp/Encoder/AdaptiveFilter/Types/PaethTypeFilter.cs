namespace PngSharp.Encoder.AdaptiveFilter.Types;

internal class PaethTypeFilter : FilterBase
{
    public PaethTypeFilter(int bytesPerPixel) : base(bytesPerPixel)
    {
    }

    public override PngSpec.AdaptiveFilterTypeKind Kind => PngSpec.AdaptiveFilterTypeKind.Paeth;
    protected override byte ComputeValue(ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        var x = currentRowBuffer[currentIndex];
        var left = GetLeftValue(currentRowBuffer, currentIndex);
        var above = GetAboveValue(previousRowBuffer, currentIndex);
        var aboveLeft = GetAboveLeftByteValue(previousRowBuffer, currentIndex);
        
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
}