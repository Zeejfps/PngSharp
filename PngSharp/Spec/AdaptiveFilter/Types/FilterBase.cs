namespace PngSharp.Spec.AdaptiveFilter.Types;

public abstract class FilterBase : ITypeFilter
{
    private readonly int m_BytesPerPixel;
    
    protected FilterBase(int bytesPerPixel)
    {
        m_BytesPerPixel = bytesPerPixel;
    }

    public abstract AdaptiveFilterTypeKind Kind { get; }

    public void Apply(Span<byte> filteredRowBuffer, ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer)
    {
        var length = currentRowBuffer.Length;
        filteredRowBuffer[0] = (byte)Kind; 
        for (var i = 0; i < length; i++)
            filteredRowBuffer[i + 1] = ComputeValue(currentRowBuffer, previousRowBuffer, i);
    }

    public void Reverse(Span<byte> outputRow, Span<byte> currentRow, ReadOnlySpan<byte> prevRow)
    {
        var length = outputRow.Length;
        for (var i = 0; i < length; i++)
        {
            var value = ReverseValue(currentRow, prevRow, i);
            outputRow[i] = value;
            currentRow[i] = value;
        }
    }

    public abstract byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex);
    public abstract byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex);
    
    protected byte GetLeftValue(ReadOnlySpan<byte> currentRowBuffer, int currByteIndex)
    {
        if (currByteIndex < m_BytesPerPixel)
            return 0;
        return currentRowBuffer[currByteIndex - m_BytesPerPixel];
    }
    
    protected byte GetAboveValue(ReadOnlySpan<byte> previousRowBuffer, int currentIndex)
    {
        return previousRowBuffer[currentIndex];
    }
    
    protected byte GetAboveLeftByteValue(ReadOnlySpan<byte> prevRow, int currByteIndex)
    {
        if (currByteIndex < m_BytesPerPixel)
            return 0;
        return prevRow[currByteIndex - m_BytesPerPixel];
    }
}
