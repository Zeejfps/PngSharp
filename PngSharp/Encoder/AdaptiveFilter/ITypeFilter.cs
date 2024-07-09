namespace PngSharp.Encoder.AdaptiveFilter;

internal interface ITypeFilter
{
    PngSpec.AdaptiveFilterTypeKind Kind { get; }
    void Apply(Span<byte> filteredRowBuffer, ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer);
    void Reverse(Span<byte> outputRow, ReadOnlySpan<byte> span, ReadOnlySpan<byte> prevRow);
}