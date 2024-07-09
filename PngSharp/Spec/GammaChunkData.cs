namespace PngSharp.Spec;

public readonly struct GammaChunkData
{
    public uint Value { get; init; }

    public override string ToString()
    {
        return $"{nameof(Value)}: {Value}";
    }
}