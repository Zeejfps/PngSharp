using PngSharp.Decoder;
using PngSharp.Encoder;

namespace PngSharp.Api;

public static class PngApi
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

        var decodedPng = decoder.DecodedPng;
        decodedPng.Width = imageWidth;
        decodedPng.Height = imageHeight;
        decodedPng.BytesPerPixel = decoder.BytesPerPixel;
        decodedPng.ColorType = decoder.IhdrChunkData.ColorType;
        decodedPng.PixelData = pixelData;

        return decodedPng;
    }

    public static void EncodeToFile(IDecodedPng decodedPng, string pathToFile)
    {
        using var fileStream = new FileStream(pathToFile, FileMode.Create);
        EncodeToStream(decodedPng, fileStream);
    }

    public static void EncodeToStream(IDecodedPng decodedPng, Stream stream)
    {
        using var encoder = new PngEncoder(decodedPng, stream);
        encoder.Encode();
    }
}