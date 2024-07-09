using PngSharp.Encoder.AdaptiveFilter.Types;

namespace PngSharp.Encoder.AdaptiveFilter;

internal sealed class PngAdaptiveFilter
{
    private readonly int m_Width;
    private readonly int m_Height;
    private readonly int m_BytesPerPixel;
    private readonly ITypeFilter[] m_FirstRowFilterTypes;
    private readonly ITypeFilter[] m_AllFilterTypes;
    
    public PngAdaptiveFilter(int width, int height, int bytesPerPixel)
    {
        m_Width = width;
        m_Height = height;
        m_BytesPerPixel = bytesPerPixel;
        
        m_FirstRowFilterTypes = new ITypeFilter[]
        {
            new NoneTypeFilter(bytesPerPixel),
            new SubTypeFilter(bytesPerPixel),
        };
        
        m_AllFilterTypes = new ITypeFilter[]
        {
            new NoneTypeFilter(bytesPerPixel),
            new SubTypeFilter(bytesPerPixel),
            new UpTypeFilter(bytesPerPixel),
            new AverageTypeFilter(bytesPerPixel),
            new PaethTypeFilter(bytesPerPixel)
        };
    }

    public void Reverse(Stream outputStream, Stream inputStream)
    {
        // var height = m_Height;
        // var currRow = m_CurrentRow.Span;
        // var prevRow = m_PrevRow.Span;
        // var outputRow = m_OutputRow.Span;
    }

    public void Apply(Stream outputStream, Stream inputStream)
    {
        var width = m_Width;
        var height = m_Height;
        var bytesPerPixel = m_BytesPerPixel;
        var firstRowFilters = m_FirstRowFilterTypes;
        var allFilters = m_AllFilterTypes;
        
        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        var buffer = new byte[strideUnfiltered + strideUnfiltered + strideFiltered];
        var currRow = new Span<byte>(buffer, 0, strideUnfiltered);
        var prevRow = new Span<byte>(buffer, strideUnfiltered, strideUnfiltered);
        var outputRow = new Span<byte>(buffer, strideUnfiltered + strideUnfiltered, strideFiltered);

        // TODO: Handle first row more gracefully?
        inputStream.ReadExactly(currRow);
        var filter = ChooseFilter(outputRow, currRow, prevRow, firstRowFilters);
        filter.Apply(outputRow, currRow, prevRow);
        outputStream.Write(outputRow);

        var t = prevRow;
        prevRow = currRow;
        currRow = t;
 
        for (var i = 1; i < height; i++)
        {
            inputStream.ReadExactly(currRow);
            filter =  ChooseFilter(outputRow, currRow, prevRow, allFilters);
            filter.Apply(outputRow, currRow, prevRow);
            outputStream.Write(outputRow);

            t = prevRow;
            prevRow = currRow;
            currRow = t;
        }
    }

    private ITypeFilter ChooseFilter(
        Span<byte> outputRow, 
        ReadOnlySpan<byte> currentRow, 
        ReadOnlySpan<byte> prevRow, 
        IEnumerable<ITypeFilter> filters)
    {
        var score = double.MaxValue;
        
        ITypeFilter bestFilter = null;
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