using PngSharp.Common;
using PngSharp.Spec;

namespace PngSharp.Api;

public record Srgb
{
    public RenderingIntent RenderingIntent { get; set; }
}