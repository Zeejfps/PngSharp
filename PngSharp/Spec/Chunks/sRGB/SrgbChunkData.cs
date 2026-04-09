namespace PngSharp.Spec.Chunks.sRGB;

public readonly record struct SrgbChunkData
{
    public RenderingIntent RenderingIntent { get; init; }
}
