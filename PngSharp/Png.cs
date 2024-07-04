using PngSharp.Decoder;

namespace PngSharp;

public enum PixelFormat
{
    RGBA
}

public interface IDecodedPng
{
    int Width { get; }
    int Height { get; }
    PixelFormat PixelFormat { get; }
    byte[] PixelData { get; }
}

public static class Png
{
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