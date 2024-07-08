using PngSharp.Decoder;
using PngSharp.Encoder;

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
    /// How many bytes in the <seealso cref="PixelData"/> array reprsent a single pixel
    /// </summary>
    int BytesPerPixel { get; }
    
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
        return DecodeFromStream(fileStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public static IDecodedPng DecodeFromStream(Stream stream)
    {
        using var decoder = new PngDecoder(stream);
        decoder.Decode();
        var imageWidth = (int)decoder.IhdrChunkData.Width;
        var imageHeight = (int)decoder.IhdrChunkData.Height;
        var pixelData = new byte[imageWidth * imageHeight * decoder.BytesPerPixel];
        decoder.PixelDataStream.Position = 0;
        var pixelsRead = decoder.PixelDataStream.Read(pixelData);
        // TODO: verify pixelsRead matches?
        var pixelFormat = decoder.GetPixelFormat();
        
        return new DecodedPng
        {
            Width = imageWidth,
            Height = imageHeight,
            BytesPerPixel = decoder.BytesPerPixel,
            PixelFormat = pixelFormat,
            PixelData = pixelData
        };
    }

    public static void EncodeToFile(IDecodedPng decodedPng, string pathToFile)
    {
        using var fileStream = new FileStream(pathToFile, FileMode.Create);
        EncodeToStream(decodedPng, fileStream);
    }

    public static void EncodeToStream(IDecodedPng decodedPng, Stream stream)
    {
        var encoder = new PngEncoder(decodedPng, stream);
        encoder.Encode();
    }
}