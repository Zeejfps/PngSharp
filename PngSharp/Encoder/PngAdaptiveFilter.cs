using PngSharp.Encoder.AdaptiveFilterTypes;

namespace PngSharp.Encoder;

internal sealed class PngAdaptiveFilter
{
    private readonly int m_Height;
    private readonly byte[] m_Buffer;

    private readonly Memory<byte> m_CurrentRow;
    private readonly Memory<byte> m_PrevRow;
    private readonly Memory<byte> m_OutputRow;
    
    private readonly IAdaptiveFilterType[] m_FirstRowFilterTypes;
    private readonly IAdaptiveFilterType[] m_AllFilterTypes;
    
    public PngAdaptiveFilter(int width, int height, int bytesPerPixel)
    {
        m_Height = height;
        
        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        m_Buffer = new byte[strideUnfiltered + strideUnfiltered + strideFiltered];

        m_CurrentRow = new Memory<byte>(m_Buffer, 0, strideUnfiltered);
        m_PrevRow = new Memory<byte>(m_Buffer, strideUnfiltered, strideUnfiltered);
        m_OutputRow = new Memory<byte>(m_Buffer, strideUnfiltered + strideUnfiltered, strideFiltered);

        m_FirstRowFilterTypes = new IAdaptiveFilterType[]
        {
            new AdaptiveFilterTypeNone(bytesPerPixel),
            new AdaptiveFilterTypeSub(bytesPerPixel),
        };
        
        m_AllFilterTypes = new IAdaptiveFilterType[]
        {
            new AdaptiveFilterTypeNone(bytesPerPixel),
            new AdaptiveFilterTypeSub(bytesPerPixel),
            new AdaptiveFilterTypeUp(bytesPerPixel),
            new AdaptiveFilterTypeAverage(bytesPerPixel)
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
        var filter = ChooseFilter(m_FirstRowFilterTypes);
        filter.Apply(outputRow, currRow, prevRow);
        outputStream.Write(outputRow);

        var t = prevRow;
        prevRow = currRow;
        currRow = t;
 
        for (var i = 1; i < height; i++)
        {
            inputStream.ReadExactly(currRow);
            filter =  ChooseFilter(m_AllFilterTypes);
            filter.Apply(outputRow, currRow, prevRow);
            outputStream.Write(outputRow);

            t = prevRow;
            prevRow = currRow;
            currRow = t;
        }
    }

    private IAdaptiveFilterType ChooseFilter(IEnumerable<IAdaptiveFilterType> filters)
    {
        var currentRow = m_CurrentRow.Span;
        var prevRow = m_PrevRow.Span;
        var outputRow = m_OutputRow.Span;
        var score = double.MaxValue;
        
        IAdaptiveFilterType bestFilter = null;
        foreach (var filter in filters)
        {
            filter.Apply(outputRow, currentRow, prevRow);
            var thisFiltersScore = ComputeScore(outputRow);
            if (thisFiltersScore < score)
            {
                score = thisFiltersScore;
                bestFilter = filter;
            }
        }
        
        Console.WriteLine($"Best filter score: {score}, Filter: {bestFilter.Kind}");
        return bestFilter;
    }

    private double ComputeScore(ReadOnlySpan<byte> row)
    {
        var sum = 0.0;
        for (var i = 1; i < row.Length; i++)
            sum += Math.Abs(row[i]);
        return sum;
    }
}