namespace PngSharp.Encoder;

public class PngFilter
{
    private readonly byte[] m_Buffer;

    private int m_Height;
    
    private readonly Memory<byte> m_CurrentRowUnfiltered;
    private readonly Memory<byte> m_PrevRowFiltered;
    private readonly Memory<byte> m_OutputRowFiltered;
    
    private readonly IAdaptiveFilter[] m_FirstRowFilters;
    private readonly IAdaptiveFilter[] m_AdaptiveFilters;
    
    public PngFilter(int width, int height, int bytesPerPixel)
    {
        m_Height = height;
        
        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        m_Buffer = new byte[strideUnfiltered + strideFiltered + strideFiltered];

        m_CurrentRowUnfiltered = new Memory<byte>(m_Buffer, 0, strideUnfiltered);
        m_OutputRowFiltered = new Memory<byte>(m_Buffer, strideUnfiltered, strideFiltered);
        m_PrevRowFiltered = new Memory<byte>(m_Buffer, strideUnfiltered + strideFiltered, strideFiltered);

        m_FirstRowFilters = new IAdaptiveFilter[]
        {
            new AdaptiveFilterNone(bytesPerPixel),
            new AdaptiveFilterSub(bytesPerPixel),
        };
        
        m_AdaptiveFilters = new IAdaptiveFilter[]
        {
            new AdaptiveFilterNone(bytesPerPixel),
            new AdaptiveFilterSub(bytesPerPixel),
            new AdaptiveFilterUp(bytesPerPixel),
        };
    }

    public void Apply(Stream outputStream, Stream inputStream)
    {
        var height = m_Height;
        
        // TODO: Handle first row more gracefully?
        inputStream.ReadExactly(m_CurrentRowUnfiltered.Span);
        var filter = ChooseFilter(m_FirstRowFilters);
        filter.Apply(m_OutputRowFiltered.Span, m_CurrentRowUnfiltered.Span, m_PrevRowFiltered.Span);
        outputStream.Write(m_OutputRowFiltered.Span);

        for (var i = 1; i < height; i++)
        {
            inputStream.ReadExactly(m_CurrentRowUnfiltered.Span);
            filter = ChooseFilter(m_AdaptiveFilters);
            filter.Apply(m_OutputRowFiltered.Span, m_CurrentRowUnfiltered.Span, m_PrevRowFiltered.Span);
            outputStream.Write(m_OutputRowFiltered.Span);
        }
    }

    private IAdaptiveFilter ChooseFilter(IEnumerable<IAdaptiveFilter> filters)
    {
        IAdaptiveFilter bestFilter = null;
        var score = -1.0;
        foreach (var filter in filters)
        {
            filter.Apply(m_OutputRowFiltered.Span, m_CurrentRowUnfiltered.Span, m_PrevRowFiltered.Span);
            var thisFiltersScore = ComputeScore(m_OutputRowFiltered.Span);
            if (thisFiltersScore > score)
            {
                score = thisFiltersScore;
                bestFilter = filter;
            }
        }
        
        Console.WriteLine($"Best filter score: {score}");
        return bestFilter;
    }

    private double ComputeScore(ReadOnlySpan<byte> rowFiltered)
    {
        if (rowFiltered.Length == 0) return 0;

        int totalRuns = 0;
        int currentRunLength = 1;

        for (int i = 1; i < rowFiltered.Length; i++)
        {
            if (rowFiltered[i] == rowFiltered[i - 1])
            {
                currentRunLength++;
            }
            else
            {
                totalRuns += currentRunLength;
                currentRunLength = 1;
            }
        }

        totalRuns += currentRunLength; // Add the last run

        return (double)rowFiltered.Length / totalRuns;
    }
}