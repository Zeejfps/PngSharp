namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal abstract class AdaptiveFilterTypeBase : IAdaptiveFilterType
{
    private readonly int m_BytesPerPixel;
    
    protected AdaptiveFilterTypeBase(int bytesPerPixel)
    {
        m_BytesPerPixel = bytesPerPixel;
    }

    public abstract PngSpec.AdaptiveFilterTypeKind Kind { get; }

    public void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer)
    {
        var length = currentRowBuffer.Length;
        filteredRowBuffer[0] = (byte)Kind; 
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
        return previousRowBuffer[currentIndex];
    }
    
    private byte GetUpLeftByteValue(ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        if (currByteIndex < m_BytesPerPixel)
            return 0;
        return prevRow[currByteIndex - m_BytesPerPixel];
    }
}
