namespace PngSharp.Common.AdaptiveFilter;

internal interface ITypeFilter
{
    PngSpec.AdaptiveFilterTypeKind Kind { get; }
    void Apply(Span<byte> filteredRowBuffer, ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer);
    void Reverse(Span<byte> outputRow, Span<byte> currentRow, ReadOnlySpan<byte> prevRow);
}