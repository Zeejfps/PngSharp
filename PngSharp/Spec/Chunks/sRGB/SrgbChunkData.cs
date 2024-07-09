namespace PngSharp.Spec.Chunks.sRGB;

public readonly struct SrgbChunkData
{
    public RenderingIntent RenderingIntent { get; init; }

    public override string ToString()
    {
        return $"{nameof(RenderingIntent)}: {RenderingIntent}";
    }
}