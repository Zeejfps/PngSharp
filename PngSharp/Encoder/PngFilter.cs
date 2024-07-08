namespace PngSharp.Encoder;

public class PngFilter
{
    private readonly byte[] m_Buffer;

    private int m_Height;
    private bool m_IsFirstScanLine;
    private readonly int m_BytesPerPixel;

    private readonly AdaptiveFilterNone m_AdaptiveFilterNone;
    private readonly AdaptiveFilterSub m_AdaptiveFilterSub;

    private readonly Memory<byte> m_CurrentRowUnfiltered;
    private readonly Memory<byte> m_PrevRowFiltered;
    private readonly Memory<byte> m_OutputRowFiltered;
    private readonly Memory<byte> m_TempRowUnfiltered;

    private readonly IAdaptiveFilter[] m_AdaptiveFilters;
    
    public PngFilter(int width, int height, int bytesPerPixel)
    {
        m_Height = height;
        var stride = width * bytesPerPixel;
        m_Buffer = new byte[stride + stride + 1 + stride + 1 + stride + 1];

        m_CurrentRowUnfiltered = new Memory<byte>(m_Buffer, 0, stride);
        m_OutputRowFiltered = new Memory<byte>(m_Buffer, stride, stride + 1);
        m_PrevRowFiltered = new Memory<byte>(m_Buffer, stride + stride + 1, stride + 1);
            
        m_BytesPerPixel = bytesPerPixel;
        m_IsFirstScanLine = true;

        m_AdaptiveFilterNone = new AdaptiveFilterNone(bytesPerPixel);
        m_AdaptiveFilterSub = new AdaptiveFilterSub(bytesPerPixel);

        m_AdaptiveFilters = new IAdaptiveFilter[]
        {
            m_AdaptiveFilterNone,
            m_AdaptiveFilterSub,
        };
    }

    public void Apply(Stream inputStream)
    {
        var height = m_Height;
        for (var i = 0; i < height; i++)
        {
            inputStream.ReadExactly(m_CurrentRowUnfiltered.Span);
            var filter = ChooseFilter();
            filter.Apply(m_OutputRowFiltered.Span, m_CurrentRowUnfiltered.Span, m_PrevRowFiltered.Span);
        }
    }

    private IAdaptiveFilter ChooseFilter()
    {
        IAdaptiveFilter bestFilter = m_AdaptiveFilterNone;
        var score = 0;
        foreach (var filter in m_AdaptiveFilters)
        {
            filter.Apply(m_OutputRowFiltered.Span, m_CurrentRowUnfiltered.Span, m_PrevRowFiltered.Span);
            var thisFiltersScore = ComputeScore(m_OutputRowFiltered.Span);
            if (thisFiltersScore > score)
            {
                score = thisFiltersScore;
                bestFilter = filter;
            }
        }

        return bestFilter;
    }

    private int ComputeScore(ReadOnlySpan<byte> span)
    {
        throw new NotImplementedException();
    }
}