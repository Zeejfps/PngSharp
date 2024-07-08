namespace PngSharp.Encoder;

public interface IAdaptiveFilter
{
    PngSpec.AdaptiveFilterType Type { get; }
    void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer);
}