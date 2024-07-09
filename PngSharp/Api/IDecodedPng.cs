using PngSharp.Common;

namespace PngSharp.Api;

public interface IDecodedPng
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
    PngSpec.ColorType ColorType { get; }
    
    /// <summary>
    /// How many bytes in the <seealso cref="PixelData"/> array reprsent a single pixel
    /// </summary>
    int BytesPerPixel { get; }
    
    /// <summary>
    /// Order of bytes is determined by <seealso cref="ColorType"/>.
    /// Ex: RGBA format stores the pixels in [R,G,B,A] where each byte represents a color channel
    /// </summary>
    byte[] PixelData { get; }
    

    AncillaryChunk<PngSpec.SrgbChunkData> Srgb { get; }
    AncillaryChunk<PngSpec.GammaChunkData> Gama { get; }
}