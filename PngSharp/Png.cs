using PngSharp.Decoder;

namespace PngSharp;

public enum PixelFormat
{
    RGBA,
    RGB,
    Grayscale,
    GrayscaleWithAlpha
}

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
    PixelFormat PixelFormat { get; }
    
    /// <summary>
    /// Order of bytes is determined by <seealso cref="PixelFormat"/>.
    /// Ex: RGBA format stores the pixels in [R,G,B,A] where each byte represents a color channel
    /// </summary>
    byte[] PixelData { get; }
}

public static class Png
{
    /// <summary>
    /// Decodes a PNG image from a file
    /// </summary>
    /// <param name="pathToFile"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public static IDecodedPng DecodeFromFile(string pathToFile)
    {
        using var fileStream = new FileStream(pathToFile, FileMode.Open);
        using var decoder = new PngDecoder(fileStream);
        decoder.Decode();
        var imageWidth = (int)decoder.IhdrChunkData.Width;
        var imageHeight = (int)decoder.IhdrChunkData.Height;
        var pixelData = new byte[imageWidth * imageHeight * decoder.BytesPerPixel];
        decoder.PixelDataStream.Position = 0;
        var pixelsRead = decoder.PixelDataStream.Read(pixelData);
        // TODO: verify pixelsRead matches?
        
        return new DecodedPng
        {
            Width = imageWidth,
            Height = imageHeight,
            PixelFormat = PixelFormat.RGBA,
            PixelData = pixelData
        };
    }
}