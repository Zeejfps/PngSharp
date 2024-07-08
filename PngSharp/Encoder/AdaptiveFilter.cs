namespace PngSharp.Encoder;

public abstract class AdaptiveFilter : IAdaptiveFilter
{
    protected readonly int m_BytesPerPixel;

    private bool m_IsFirstScanLine;

    protected AdaptiveFilter(int bytesPerPixel)
    {
        m_BytesPerPixel = bytesPerPixel;
    }

    public abstract PngSpec.AdaptiveFilterType Type { get; }

    public void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer)
    {
        var length = currentRowBuffer.Length;
        for (var i = 0; i < length; i++)
            filteredRowBuffer[i + 1] = ComputeValue(currentRowBuffer, previousRowBuffer, i);
    }

    protected abstract byte ComputeValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex);
    
    protected byte GetLeftValue(Span<byte> currentRowBuffer, int currByteIndex)
    {
        if (currByteIndex < m_BytesPerPixel)
            return 0;
        return currentRowBuffer[currByteIndex - m_BytesPerPixel];
    }
    
    protected int GetAboveValue(Span<byte> previousRowBuffer, int currentIndex)
    {
        // if (m_IsFirstScanLine)
        //     return 0;
        return previousRowBuffer[currentIndex];
    }
    
    private byte GetUpLeftByteValue(ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        if (m_IsFirstScanLine || currByteIndex < m_BytesPerPixel)
            return 0;
        return prevRow[currByteIndex - m_BytesPerPixel];
    }
}
