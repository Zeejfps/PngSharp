using PngSharp.Common;

namespace PngSharp.Api;

public record Srgb
{
    public PngSpec.RenderingIntent RenderingIntent { get; set; }
}