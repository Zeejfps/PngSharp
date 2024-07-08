namespace PngSharp.Encoder.AdaptiveFilterTypes;

internal interface IAdaptiveFilterType
{
    PngSpec.AdaptiveFilterTypeKind Kind { get; }
    void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer);
}