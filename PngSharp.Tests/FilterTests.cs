using PngSharp.Spec.AdaptiveFilter.Types;
using Xunit;

namespace PngSharp.Tests;

public class FilterTests
{
    private static readonly byte[] TestRow = [10, 20, 30, 40, 50, 60];
    private static readonly byte[] PrevRow = [5, 15, 25, 35, 45, 55];
    private const int BytesPerPixel = 3;

    private static void AssertFilterRoundTrip(FilterBase filter, byte[] row, byte[] prevRow)
    {
        var filteredBuf = new byte[row.Length + 1];
        filter.Apply(filteredBuf, row, prevRow);

        // Copy filtered data (skip type byte) into a mutable array for Reverse
        var filteredData = filteredBuf[1..];
        var outputRow = new byte[row.Length];
        filter.Reverse(outputRow, filteredData, prevRow);

        Assert.Equal(row, outputRow);
    }

    [Fact]
    public void NoneFilter_RoundTrip_RestoresOriginalRow()
    {
        AssertFilterRoundTrip(new NoneTypeFilter(BytesPerPixel), TestRow, PrevRow);
    }

    [Fact]
    public void SubFilter_RoundTrip_RestoresOriginalRow()
    {
        AssertFilterRoundTrip(new SubTypeFilter(BytesPerPixel), TestRow, PrevRow);
    }

    [Fact]
    public void UpFilter_RoundTrip_RestoresOriginalRow()
    {
        AssertFilterRoundTrip(new UpTypeFilter(BytesPerPixel), TestRow, PrevRow);
    }

    [Fact]
    public void AverageFilter_RoundTrip_RestoresOriginalRow()
    {
        AssertFilterRoundTrip(new AverageTypeFilter(BytesPerPixel), TestRow, PrevRow);
    }

    [Fact]
    public void PaethFilter_RoundTrip_RestoresOriginalRow()
    {
        AssertFilterRoundTrip(new PaethTypeFilter(BytesPerPixel), TestRow, PrevRow);
    }

    [Fact]
    public void SubFilter_HandComputed_MatchesSpec()
    {
        // RFC 2083 §6.2: Filt(x) = Orig(x) - Orig(a), where a is bpp bytes back or 0
        // bpp=3, row=[10,20,30,40,50,60], prev=[5,15,25,35,45,55]
        // Bytes 0-2: a=0 → [10,20,30]. Bytes 3-5: a=row[i-3] → [30,30,30]
        var filter = new SubTypeFilter(BytesPerPixel);
        var filteredBuf = new byte[TestRow.Length + 1];
        filter.Apply(filteredBuf, TestRow, PrevRow);

        byte[] expected = [10, 20, 30, 30, 30, 30];
        Assert.Equal(expected, filteredBuf[1..]);
    }

    [Fact]
    public void UpFilter_HandComputed_MatchesSpec()
    {
        // RFC 2083 §6.3: Filt(x) = Orig(x) - Prior(x)
        // Each byte: row[i] - prev[i] → all 5s
        var filter = new UpTypeFilter(BytesPerPixel);
        var filteredBuf = new byte[TestRow.Length + 1];
        filter.Apply(filteredBuf, TestRow, PrevRow);

        byte[] expected = [5, 5, 5, 5, 5, 5];
        Assert.Equal(expected, filteredBuf[1..]);
    }

    [Fact]
    public void AverageFilter_HandComputed_MatchesSpec()
    {
        // RFC 2083 §6.4: Filt(x) = Orig(x) - floor((a + b) / 2)
        // Byte 0: 10 - floor((0+5)/2) = 8
        // Byte 1: 20 - floor((0+15)/2) = 13
        // Byte 2: 30 - floor((0+25)/2) = 18
        // Byte 3: 40 - floor((10+35)/2) = 18
        // Byte 4: 50 - floor((20+45)/2) = 18
        // Byte 5: 60 - floor((30+55)/2) = 18
        var filter = new AverageTypeFilter(BytesPerPixel);
        var filteredBuf = new byte[TestRow.Length + 1];
        filter.Apply(filteredBuf, TestRow, PrevRow);

        byte[] expected = [8, 13, 18, 18, 18, 18];
        Assert.Equal(expected, filteredBuf[1..]);
    }

    [Fact]
    public void PaethFilter_HandComputed_MatchesSpec()
    {
        // RFC 2083 §6.6: Filt(x) = Orig(x) - PaethPredictor(a, b, c)
        // bpp=1, row=[100,150], prev=[50,80]
        //
        // Byte 0: a=0, b=50, c=0. p=50. pa=50, pb=0, pc=50 → predictor=b=50. filtered=50
        // Byte 1: a=100, b=80, c=50. p=130. pa=30, pb=50, pc=80 → predictor=a=100. filtered=50
        var filter = new PaethTypeFilter(1);
        byte[] row = [100, 150];
        byte[] prevRow = [50, 80];

        var filteredBuf = new byte[row.Length + 1];
        filter.Apply(filteredBuf, row, prevRow);

        byte[] expected = [50, 50];
        Assert.Equal(expected, filteredBuf[1..]);
    }
}
