using PngSharp.Spec;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Api;

public record Srgb
{
    public RenderingIntent RenderingIntent { get; set; }
}