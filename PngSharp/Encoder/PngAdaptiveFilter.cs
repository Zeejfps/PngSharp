namespace PngSharp.Encoder;

public class PngAdaptiveFilter
{
    private readonly byte[] m_Buffer;

    private int m_Height;
    
    private readonly Memory<byte> m_CurrentRow;
    private readonly Memory<byte> m_PrevRow;
    private readonly Memory<byte> m_OutputRow;
    
    private readonly IAdaptiveFilter[] m_FirstRowFilters;
    private readonly IAdaptiveFilter[] m_AllFilters;
    
    public PngAdaptiveFilter(int width, int height, int bytesPerPixel)
    {
        m_Height = height;
        
        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        m_Buffer = new byte[strideUnfiltered + strideFiltered + strideFiltered];

        m_CurrentRow = new Memory<byte>(m_Buffer, 0, strideUnfiltered);
        m_OutputRow = new Memory<byte>(m_Buffer, strideUnfiltered, strideFiltered);
        m_PrevRow = new Memory<byte>(m_Buffer, strideUnfiltered + strideFiltered, strideFiltered);

        m_FirstRowFilters = new IAdaptiveFilter[]
        {
            new AdaptiveFilterNone(bytesPerPixel),
            new AdaptiveFilterSub(bytesPerPixel),
        };
        
        m_AllFilters = new IAdaptiveFilter[]
        {
            new AdaptiveFilterNone(bytesPerPixel),
            new AdaptiveFilterSub(bytesPerPixel),
            new AdaptiveFilterUp(bytesPerPixel),
        };
    }

    public void Apply(Stream outputStream, Stream inputStream)
    {
        var height = m_Height;
        var currRow = m_CurrentRow.Span;
        var prevRow = m_PrevRow.Span;
        var outputRow = m_OutputRow.Span;

        // TODO: Handle first row more gracefully?
        inputStream.ReadExactly(currRow);
        var filter = ChooseFilter(m_FirstRowFilters);
        filter.Apply(outputRow, currRow, prevRow[1..]);
        outputStream.Write(outputRow);

        var t = prevRow;
        prevRow = outputRow;
        outputRow = t;
        
        for (var i = 1; i < height; i++)
        {
            inputStream.ReadExactly(currRow);
            filter = ChooseFilter(m_AllFilters);
            filter.Apply(outputRow, currRow, prevRow[1..]);
            outputStream.Write(outputRow);
                
            t = prevRow;
            prevRow = outputRow;
            outputRow = t;
        }
    }

    private IAdaptiveFilter ChooseFilter(IEnumerable<IAdaptiveFilter> filters)
    {
        var outputRow = m_OutputRow.Span;
        IAdaptiveFilter bestFilter = null;
        var score = -1.0;
        foreach (var filter in filters)
        {
            filter.Apply(outputRow, m_CurrentRow.Span, m_PrevRow.Span);
            var thisFiltersScore = ComputeScore(outputRow);
            if (thisFiltersScore > score)
            {
                score = thisFiltersScore;
                bestFilter = filter;
            }
        }
        
        Console.WriteLine($"Best filter score: {score}, Filter: {bestFilter.Type}");
        return bestFilter;
    }

    private double ComputeScore(ReadOnlySpan<byte> row)
    {
        if (row.Length == 0) return 0;

        int totalRuns = 0;
        int currentRunLength = 1;

        for (int i = 1; i < row.Length; i++)
        {
            if (row[i] == row[i - 1])
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

        return (double)row.Length / totalRuns;
    }
}