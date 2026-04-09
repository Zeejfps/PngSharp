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
    /// <exception cref="Exceptions.PngFormatException">Thrown when the PNG has an invalid format</exception>
    /// <exception cref="Exceptions.PngCorruptException">Thrown when the PNG data is corrupted</exception>
    public static IRawPng DecodeFromByteArray(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        return DecodeFromStream(memoryStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a file
    /// </summary>
    /// <param name="pathToFile"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    /// <exception cref="Exceptions.PngFormatException">Thrown when the PNG has an invalid format</exception>
    /// <exception cref="Exceptions.PngCorruptException">Thrown when the PNG data is corrupted</exception>
    public static IRawPng DecodeFromFile(string pathToFile)
    {
        using var fileStream = FileSystem.OpenFile(pathToFile);
        return DecodeFromStream(fileStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a stream
    /// </summary>
    /// <param name="inputStream"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    /// <exception cref="Exceptions.PngFormatException">Thrown when the PNG has an invalid format</exception>
    /// <exception cref="Exceptions.PngCorruptException">Thrown when the PNG data is corrupted</exception>
    public static IRawPng DecodeFromStream(Stream inputStream)
    {
        var reader = new PngReader(inputStream);
        using var decoder = new PngDecoder(reader, Logger);
        decoder.Decode();
        var ihdr = decoder.IhdrChunkData;
        var pixelData = new byte[(int)ihdr.Width * (int)ihdr.Height * ihdr.GetBytesPerPixel()];
        decoder.PixelDataStream.Position = 0;
        var pixelsRead = decoder.PixelDataStream.Read(pixelData);
        // TODO: verify pixelsRead matches?

        return new RawPng
        {
            Ihdr = ihdr,
            PixelData = pixelData,
            Srgb = decoder.Srgb,
            Gama = decoder.Gama,
            Phys = decoder.Phys,
        };
    }

    /// <summary>
    /// Encodes a PNG image to a file
    /// </summary>
    /// <param name="rawPng">The PNG image data to encode</param>
    /// <param name="pathToFile">The path to write the PNG file to</param>
    public static void EncodeToFile(IRawPng rawPng, string pathToFile)
    {
        using var fileStream = FileSystem.CreateFile(pathToFile);
        EncodeToStream(rawPng, fileStream);
    }

    /// <summary>
    /// Encodes a PNG image to a stream
    /// </summary>
    /// <param name="rawPng">The PNG image data to encode</param>
    /// <param name="stream">The stream to write the encoded PNG data to</param>
    public static void EncodeToStream(IRawPng rawPng, Stream stream)
    {
        var crc32 = new PngCrc32();
        using var writer = new PngWriter(stream, crc32);
        using var encoder = new PngEncoder(rawPng, writer, Logger);
        encoder.Encode();
    }

    /// <summary>
    /// Creates a grayscale PNG image (1 byte per pixel)
    /// </summary>
    /// <param name="width">Width of the image in pixels</param>
    /// <param name="height">Height of the image in pixels</param>
    /// <param name="pixels">Raw pixel data in grayscale format</param>
    /// <returns>A new PNG image with grayscale color type</returns>
    public static IRawPng CreateGrayscale(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Ihdr = new IhdrChunkData
            {
                Width = (uint)width,
                Height = (uint)height,
                BitDepth = 8,
                ColorType = ColorType.Grayscale,
                CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
                FilterMethod = FilterMethod.AdaptiveFiltering,
                InterlaceMethod = InterlaceMethod.None,
            },
            PixelData = pixels
        };
    }

    /// <summary>
    /// Creates a grayscale PNG image with an alpha channel (2 bytes per pixel)
    /// </summary>
    /// <param name="width">Width of the image in pixels</param>
    /// <param name="height">Height of the image in pixels</param>
    /// <param name="pixels">Raw pixel data in grayscale+alpha format [G,A]</param>
    /// <returns>A new PNG image with grayscale+alpha color type</returns>
    public static IRawPng CreateGrayscaleWithAlpha(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Ihdr = new IhdrChunkData
            {
                Width = (uint)width,
                Height = (uint)height,
                BitDepth = 8,
                ColorType = ColorType.GrayscaleWithAlpha,
                CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
                FilterMethod = FilterMethod.AdaptiveFiltering,
                InterlaceMethod = InterlaceMethod.None,
            },
            PixelData = pixels
        };
    }

    /// <summary>
    /// Creates an RGB PNG image (3 bytes per pixel)
    /// </summary>
    /// <param name="width">Width of the image in pixels</param>
    /// <param name="height">Height of the image in pixels</param>
    /// <param name="pixels">Raw pixel data in RGB format [R,G,B]</param>
    /// <returns>A new PNG image with true color type</returns>
    public static IRawPng CreateRgb(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Ihdr = new IhdrChunkData
            {
                Width = (uint)width,
                Height = (uint)height,
                BitDepth = 8,
                ColorType = ColorType.TrueColor,
                CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
                FilterMethod = FilterMethod.AdaptiveFiltering,
                InterlaceMethod = InterlaceMethod.None,
            },
            PixelData = pixels
        };
    }

    /// <summary>
    /// Creates an RGBA PNG image (4 bytes per pixel)
    /// </summary>
    /// <param name="width">Width of the image in pixels</param>
    /// <param name="height">Height of the image in pixels</param>
    /// <param name="pixels">Raw pixel data in RGBA format [R,G,B,A]</param>
    /// <returns>A new PNG image with true color+alpha type</returns>
    public static IRawPng CreateRgba(int width, int height, byte[] pixels)
    {
        return new RawPng
        {
            Ihdr = new IhdrChunkData
            {
                Width = (uint)width,
                Height = (uint)height,
                BitDepth = 8,
                ColorType = ColorType.TrueColorWithAlpha,
                CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
                FilterMethod = FilterMethod.AdaptiveFiltering,
                InterlaceMethod = InterlaceMethod.None,
            },
            PixelData = pixels
        };
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
}