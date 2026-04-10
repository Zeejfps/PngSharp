namespace PngSharp.Spec.Chunks.sGAMA;

public readonly record struct GammaChunkData
{
    private const double ScaleFactor = 100000.0;

    public uint Value { get; init; }

    public double ToDouble() => Value / ScaleFactor;

    public static GammaChunkData FromDouble(double gamma)
    {
        return new GammaChunkData { Value = (uint)Math.Round(gamma * ScaleFactor) };
    }
}
