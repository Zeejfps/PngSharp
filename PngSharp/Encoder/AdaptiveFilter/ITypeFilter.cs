namespace PngSharp.Encoder.AdaptiveFilter;

internal interface ITypeFilter
{
    PngSpec.AdaptiveFilterTypeKind Kind { get; }
    void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer);
}