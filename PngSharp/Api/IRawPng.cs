using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Api;

public interface IRawPng
{
    /// <summary>
    /// Width of the image in pixels
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// Height of the image in pixels
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// The ordering of bytes inside the <seealso cref="PixelData"/> array
    /// </summary>
    ColorType ColorType { get; }
    
    /// <summary>
    /// How many bytes in the <seealso cref="PixelData"/> array represent a single pixel
    /// </summary>
    int BytesPerPixel { get; }
    
    /// <summary>
    /// Order of bytes is determined by <seealso cref="ColorType"/>.
    /// Ex: RGBA format stores the pixels in [R,G,B,A] where each byte represents a color channel
    /// </summary>
    byte[] PixelData { get; }
    
    AncillaryChunk<SrgbChunkData> Srgb { get; }
    AncillaryChunk<GammaChunkData> Gama { get; }
    AncillaryChunk<PhysChunkData> Phys { get; }
}