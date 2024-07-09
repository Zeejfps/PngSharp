namespace PngSharp.Spec;

public readonly struct SrgbChunkData
{
    public RenderingIntent RenderingIntent { get; init; }

    public override string ToString()
    {
        return $"{nameof(RenderingIntent)}: {RenderingIntent}";
    }
}