using PngSharp.Decoder;
using PngSharp.Encoder;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Api;

public static class Png
{
    public static ILogger Logger { get; set; } = new NullLogger();
    public static IFileSystem FileSystem { get; set; } = new OsFileSystem();


    /// <summary>
    /// Decodes a PNG image from a byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public static IDecodedPng DecodeFromByteArray(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        return DecodeFromStream(memoryStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a file
    /// </summary>
    /// <param name="pathToFile"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public static IDecodedPng DecodeFromFile(string pathToFile)
    {
        using var fileStream = FileSystem.OpenFile(pathToFile);
        return DecodeFromStream(fileStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a stream
    /// </summary>
    /// <param name="inputStream"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public static IDecodedPng DecodeFromStream(Stream inputStream)
    {
        var reader = new PngReader(inputStream);
        using var decoder = new PngDecoder(reader, Logger);
        decoder.Decode();
        var imageWidth = (int)decoder.IhdrChunkData.Width;
        var imageHeight = (int)decoder.IhdrChunkData.Height;
        var pixelData = new byte[imageWidth * imageHeight * decoder.BytesPerPixel];
        decoder.PixelDataStream.Position = 0;
        var pixelsRead = decoder.PixelDataStream.Read(pixelData);
        // TODO: verify pixelsRead matches?

        var decodedPng = decoder.RawPng;
        decodedPng.BytesPerPixel = decoder.BytesPerPixel;
        decodedPng.PixelData = pixelData;

        return decodedPng;
    }

    public static void EncodeToFile(IDecodedPng decodedPng, string pathToFile)
    {
        using var fileStream = FileSystem.CreateFile(pathToFile);
        EncodeToStream(decodedPng, fileStream);
    }

    public static void EncodeToStream(IDecodedPng decodedPng, Stream stream)
    {
        var crc32 = new PngCrc32();
        using var writer = new PngWriter(stream, crc32);
        using var encoder = new PngEncoder(decodedPng, writer, Logger);
        encoder.Encode();
    }
    
    private sealed class NullLogger : ILogger
    {
        public void Debug(string message) { }
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Error(string message) { }
    }

    private sealed class OsFileSystem : IFileSystem
    {
        public Stream CreateFile(string pathToFile)
        {
            return new FileStream(pathToFile, FileMode.Create);
        }

        public Stream OpenFile(string pathToFile)
        {
            return new FileStream(pathToFile, FileMode.Open);
        }
    }

    public static IDecodedPng CreateGrayscale(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Width = width,
            Height = height,
            BytesPerPixel = 1,
            ColorType = ColorType.Grayscale,
            PixelData = pixels
        };
    }

    public static IDecodedPng CreateGrayscaleWithAlpha(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Width = width,
            Height = height,
            BytesPerPixel = 2,
            ColorType = ColorType.GrayscaleWithAlpha,
            PixelData = pixels
        };
    }

    public static IDecodedPng CreateRgb(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Width = width,
            Height = height,
            BytesPerPixel = 3,
            ColorType = ColorType.TrueColor,
            PixelData = pixels
        };
    }

    public static IDecodedPng CreateRgba(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Width = width,
            Height = height,
            BytesPerPixel = 4,
            ColorType = ColorType.TrueColorWithAlpha,
            PixelData = pixels
        };
    }
}