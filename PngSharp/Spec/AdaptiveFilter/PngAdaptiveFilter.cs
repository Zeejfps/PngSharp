using PngSharp.Spec.AdaptiveFilter.Types;

namespace PngSharp.Spec.AdaptiveFilter;

internal sealed class PngAdaptiveFilter
{
    private readonly int m_Width;
    private readonly int m_Height;
    private readonly int m_BytesPerPixel;
    private readonly Dictionary<AdaptiveFilterTypeKind, ITypeFilter> m_AllFilterTypes = new();
    
    public PngAdaptiveFilter(int width, int height, int bytesPerPixel)
    {
        m_Width = width;
        m_Height = height;
        m_BytesPerPixel = bytesPerPixel;

        var allFilterTypes = new ITypeFilter[]
        {
            new NoneTypeFilter(bytesPerPixel),
            new SubTypeFilter(bytesPerPixel),
            new UpTypeFilter(bytesPerPixel),
            new AverageTypeFilter(bytesPerPixel),
            new PaethTypeFilter(bytesPerPixel)
        };

        foreach (var filter in allFilterTypes)
            m_AllFilterTypes.Add(filter.Kind, filter);
    }

    public void Reverse(Stream outputStream, Stream inputStream)
    {
        var width = m_Width;
        var height = m_Height;
        var bytesPerPixel = m_BytesPerPixel;

        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        var buffer = new byte[strideUnfiltered + strideUnfiltered + strideFiltered];
        var outputRow = new Span<byte>(buffer, 0, strideUnfiltered);
        var prevRow = new Span<byte>(buffer, strideUnfiltered, strideUnfiltered);
        var currRow = new Span<byte>(buffer, strideUnfiltered + strideUnfiltered, strideFiltered);

        // TODO: Handle first row more gracefully?
        inputStream.ReadExactly(currRow);
        var filterKind = (AdaptiveFilterTypeKind)currRow[0];
        var filter = GetFilterByKind(filterKind);
        filter.Reverse(outputRow, currRow[1..], prevRow);
        outputStream.Write(outputRow);
        
        var t = prevRow;
        prevRow = outputRow;
        outputRow = t;

        for (var i = 1; i < height; i++)
        {
            inputStream.ReadExactly(currRow);
            filterKind = (AdaptiveFilterTypeKind)currRow[0];
            filter = GetFilterByKind(filterKind);
            filter.Reverse(outputRow, currRow[1..], prevRow);
            outputStream.Write(outputRow);
            t = prevRow;
            prevRow = outputRow;
            outputRow = t;
        }
    }

    private ITypeFilter GetFilterByKind(AdaptiveFilterTypeKind kind)
    {
        return m_AllFilterTypes[kind];
    }

    public void Apply(Stream outputStream, Stream inputStream)
    {
        var width = m_Width;
        var height = m_Height;
        var bytesPerPixel = m_BytesPerPixel;
        var allFilters = m_AllFilterTypes.Values;
        
        var strideUnfiltered = width * bytesPerPixel;
        var strideFiltered = strideUnfiltered + 1;
        var buffer = new byte[strideUnfiltered + strideUnfiltered + strideFiltered];
        var currRow = new Span<byte>(buffer, 0, strideUnfiltered);
        var prevRow = new Span<byte>(buffer, strideUnfiltered, strideUnfiltered);
        var outputRow = new Span<byte>(buffer, strideUnfiltered + strideUnfiltered, strideFiltered);
        
        for (var i = 0; i < height; i++)
        {
            inputStream.ReadExactly(currRow);
            var filter =  ChooseFilter(outputRow, currRow, prevRow, allFilters);
            filter.Apply(outputRow, currRow, prevRow);
            outputStream.Write(outputRow);

            var t = prevRow;
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