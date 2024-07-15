namespace PngSharp.Spec.AdaptiveFilter;

internal interface ITypeFilter
{
    AdaptiveFilterTypeKind Kind { get; }
    void Apply(Span<byte> filteredRowBuffer, ReadOnlySpan<byte> currentRowBuffer, ReadOnlySpan<byte> previousRowBuffer);
    void Reverse(Span<byte> outputRow, Span<byte> currentRow, ReadOnlySpan<byte> prevRow);
    byte ComputeValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex);
    byte ReverseValue(ReadOnlySpan<byte> currentRow, ReadOnlySpan<byte> prevRow, int currByteIndex);
}