namespace PngSharp.Encoder;

public class PngFilter
{
    private readonly byte[] m_CurrentUnfilteredScanLine;
    private byte[] m_PreviousFilteredScanLine;
    private byte[] m_CurrentFilteredScanLine;

    private bool m_IsFirstScanLine;
    private readonly int m_BytesPerPixel;

    private readonly AdaptiveFilterNone m_AdaptiveFilterNone;
    private readonly AdaptiveFilterSub m_AdaptiveFilterSub;
    
    public PngFilter(int width, int bytesPerPixel)
    {
        var stride = width * bytesPerPixel;
        m_CurrentUnfilteredScanLine = new byte[stride];
        m_PreviousFilteredScanLine = new byte[stride + 1];
        m_CurrentFilteredScanLine = new byte[stride + 1];
        m_BytesPerPixel = bytesPerPixel;
        m_IsFirstScanLine = true;

        m_AdaptiveFilterNone = new AdaptiveFilterNone(bytesPerPixel);
        m_AdaptiveFilterSub = new AdaptiveFilterSub(bytesPerPixel);
    }

    private void ApplyFilter(Span<byte> filteredRowBuffer, PngSpec.AdaptiveFilterType filterType, ReadOnlySpan<byte> unfilteredRowBuffer)
    {
        IAdaptiveFilter filter = filterType switch
        {
            PngSpec.AdaptiveFilterType.None => m_AdaptiveFilterNone,
            PngSpec.AdaptiveFilterType.Sub => m_AdaptiveFilterSub,
            PngSpec.AdaptiveFilterType.Up => m_AdaptiveFilterNone,
            PngSpec.AdaptiveFilterType.Average => m_AdaptiveFilterNone,
            PngSpec.AdaptiveFilterType.Paeth => m_AdaptiveFilterNone,
            _ => throw new ArgumentOutOfRangeException()
        };

        filter.Apply(m_CurrentFilteredScanLine, m_CurrentUnfilteredScanLine, m_PreviousFilteredScanLine);
    }
    
    private byte GetUpLeftByteValue(int currByteIndex)
    {
        if (m_IsFirstScanLine || currByteIndex < m_BytesPerPixel)
            return 0;
        return m_PreviousFilteredScanLine[currByteIndex - m_BytesPerPixel];
    }

    private byte GetUpValue(int currByteIndex)
    {
        if (m_IsFirstScanLine)
            return 0;
        return m_PreviousFilteredScanLine[currByteIndex];
    }
}