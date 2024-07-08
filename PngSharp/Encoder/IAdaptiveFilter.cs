namespace PngSharp.Encoder;

public interface IAdaptiveFilter
{
    void Apply(Span<byte> filteredRowBuffer, Span<byte> currentRowBuffer, Span<byte> previousRowBuffer);
}