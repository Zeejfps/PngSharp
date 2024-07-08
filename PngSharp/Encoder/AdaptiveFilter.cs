namespace PngSharp.Encoder;

public abstract class AdaptiveFilter : IAdaptiveFilter
{
    protected readonly int m_BytesPerPixel;

    protected AdaptiveFilter(int bytesPerPixel)
    {
        m_BytesPerPixel = bytesPerPixel;
    }

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
    
    protected int GetAboveValue(Span<byte> currentRowBuffer, Span<byte> previousRowBuffer, int currentIndex)
    {
        // if (m_IsFirstScanLine)
        //     return 0;
        return previousRowBuffer[currentIndex];
    }
}