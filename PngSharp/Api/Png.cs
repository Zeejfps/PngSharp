using PngSharp.Decoder;
using PngSharp.Encoder;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;

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
            Plte = decoder.Plte,
            Trns = decoder.Trns,
            Srgb = decoder.Srgb,
            Gama = decoder.Gama,
            Phys = decoder.Phys,
            Chrm = decoder.Chrm,
            Time = decoder.Time,
            Bkgd = decoder.Bkgd,
            Sbit = decoder.Sbit,
            Iccp = decoder.Iccp,
            Exif = decoder.Exif,
            TxtChunks = decoder.TxtChunks,
            ZTxtChunks = decoder.ZTxtChunks,
            ITxtChunks = decoder.ITxtChunks,
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
    /// Encodes a PNG image to a byte array
    /// </summary>
    /// <param name="rawPng">The PNG image data to encode</param>
    /// <returns>The encoded PNG data as a byte array</returns>
    public static byte[] EncodeToByteArray(IRawPng rawPng)
    {
        using var memoryStream = new MemoryStream();
        EncodeToStream(rawPng, memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a new builder for constructing an IRawPng instance
    /// </summary>
    /// <returns>A new builder instance</returns>
    public static IRawPngBuilder Builder() => new RawPngBuilder();

    /// <summary>
    /// Creates a grayscale PNG image (1 byte per pixel)
    /// </summary>
    /// <param name="width">Width of the image in pixels</param>
    /// <param name="height">Height of the image in pixels</param>
    /// <param name="pixels">Raw pixel data in grayscale format</param>
    /// <returns>A new PNG image with grayscale color type</returns>
    public static IRawPng CreateGrayscale(int width, int height, byte[] pixels)
    {
        return Builder()
            .WithIhdr(CreateIhdr(width, height, ColorType.Grayscale))
            .WithPixelData(pixels)
            .Build();
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
        return Builder()
            .WithIhdr(CreateIhdr(width, height, ColorType.GrayscaleWithAlpha))
            .WithPixelData(pixels)
            .Build();
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
        return Builder()
            .WithIhdr(CreateIhdr(width, height, ColorType.TrueColor))
            .WithPixelData(pixels)
            .Build();
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
        return Builder()
            .WithIhdr(CreateIhdr(width, height, ColorType.TrueColorWithAlpha))
            .WithPixelData(pixels)
            .Build();
    }

    private static IhdrChunkData CreateIhdr(int width, int height, ColorType colorType)
    {
        return new IhdrChunkData
        {
            Width = (uint)width,
            Height = (uint)height,
            BitDepth = 8,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }
    
    /// <summary>
    /// Returns the image dimensions as a <see cref="PngSize"/>
    /// </summary>
    /// <param name="png">The PNG image to query</param>
    /// <returns>The width and height of the image</returns>
    public static PngSize GetDimensions(this IRawPng png)
    {
        return new PngSize((int)png.Ihdr.Width, (int)png.Ihdr.Height);
    }

    /// <summary>
    /// Returns the total byte count of the decompressed pixel data
    /// </summary>
    /// <param name="png">The PNG image to query</param>
    /// <returns>The size in bytes of the pixel data</returns>
    public static long GetMemorySize(this IRawPng png)
    {
        return (long)png.Ihdr.Width * png.Ihdr.Height * png.Ihdr.GetBytesPerPixel();
    }

    /// <summary>
    /// Returns true if the image has an alpha channel, either natively or via a tRNS chunk
    /// </summary>
    /// <param name="png">The PNG image to query</param>
    /// <returns>True if the image has transparency information</returns>
    public static bool HasAlphaChannel(this IRawPng png)
    {
        return png.Ihdr.ColorType is ColorType.GrayscaleWithAlpha or ColorType.TrueColorWithAlpha
               || png.Trns.HasValue;
    }

    /// <summary>
    /// Returns true if the image has a palette (PLTE chunk)
    /// </summary>
    /// <param name="png">The PNG image to query</param>
    /// <returns>True if the image contains a palette</returns>
    public static bool HasPalette(this IRawPng png)
    {
        return png.Plte.HasValue;
    }

    /// <summary>
    /// Returns true if the image color type is grayscale or grayscale with alpha
    /// </summary>
    /// <param name="png">The PNG image to query</param>
    /// <returns>True if the image is grayscale</returns>
    public static bool IsGrayscale(this IRawPng png)
    {
        return png.Ihdr.ColorType is ColorType.Grayscale or ColorType.GrayscaleWithAlpha;
    }

    /// <summary>
    /// Returns the effective file gamma resolved from chunk precedence: sRGB > gAMA.
    /// Returns null if no gamma information is available (no sRGB or gAMA chunk),
    /// or if only an iCCP chunk is present (ICC profile parsing is not supported).
    /// </summary>
    public static double? GetFileGamma(this IRawPng png)
    {
        if (png.Srgb.HasValue)
            return 1.0 / 2.2;
        if (png.Gama.HasValue)
            return png.Gama.Value.ToDouble();
        return null;
    }

    /// <summary>
    /// Applies PNG spec gamma correction to the pixel data.
    /// Formula: output = input ^ (1.0 / (fileGamma * displayGamma))
    /// Alpha channels are never corrected. For indexed color images, the pixel data
    /// is expanded to RGB (3 bytes per pixel) with corrected palette values.
    /// </summary>
    /// <param name="png">The PNG image</param>
    /// <param name="displayGamma">The display gamma exponent (default 2.2 for sRGB monitors)</param>
    /// <returns>A new byte array with gamma-corrected pixel data</returns>
    public static byte[] ApplyGammaCorrection(this IRawPng png, double displayGamma = 2.2)
    {
        GammaUtils.GuardBitDepth(png);
        if (displayGamma <= 0)
            throw new ArgumentOutOfRangeException(nameof(displayGamma), "Display gamma must be greater than zero.");

        var fileGamma = png.GetFileGamma()
            ?? throw new InvalidOperationException(
                "No gamma information available. The image has no sRGB or gAMA chunk.");

        var exponent = 1.0 / (fileGamma * displayGamma);
        var lut = GammaUtils.BuildLut(c => Math.Pow(c, exponent));
        return GammaUtils.ApplyLutToPixels(png, lut);
    }

    /// <summary>
    /// Converts pixel data to linear light (gamma 1.0).
    /// Uses the precise sRGB piecewise transfer function when an sRGB chunk is present,
    /// otherwise uses power-law inversion from the gAMA chunk.
    /// Alpha channels are never corrected. For indexed color images, the pixel data
    /// is expanded to RGB (3 bytes per pixel).
    /// </summary>
    public static byte[] ToLinear(this IRawPng png)
    {
        GammaUtils.GuardBitDepth(png);

        byte[] lut;
        if (png.Srgb.HasValue)
        {
            lut = GammaUtils.BuildLut(GammaUtils.SrgbToLinear);
        }
        else if (png.Gama.HasValue)
        {
            var fileGamma = png.Gama.Value.ToDouble();
            var exponent = 1.0 / fileGamma;
            lut = GammaUtils.BuildLut(c => Math.Pow(c, exponent));
        }
        else
        {
            throw new InvalidOperationException(
                "No gamma information available. The image has no sRGB or gAMA chunk.");
        }

        return GammaUtils.ApplyLutToPixels(png, lut);
    }

    /// <summary>
    /// Converts pixel data to sRGB encoding.
    /// If the image already has an sRGB chunk, returns a clone of the pixel data.
    /// Otherwise linearizes via the gAMA value and applies the linear-to-sRGB transfer function.
    /// Alpha channels are never corrected. For indexed color images, the pixel data
    /// is expanded to RGB (3 bytes per pixel).
    /// </summary>
    public static byte[] ToSrgb(this IRawPng png)
    {
        GammaUtils.GuardBitDepth(png);

        if (png.Srgb.HasValue)
        {
            if (png.Ihdr.ColorType == ColorType.IndexedColor)
                return GammaUtils.ExpandIndexedToRgb(png, null);
            return (byte[])png.PixelData.Clone();
        }

        if (png.Gama.HasValue)
        {
            var fileGamma = png.Gama.Value.ToDouble();
            var exponent = 1.0 / fileGamma;
            var lut = GammaUtils.BuildLut(c => GammaUtils.LinearToSrgb(Math.Pow(c, exponent)));
            return GammaUtils.ApplyLutToPixels(png, lut);
        }

        throw new InvalidOperationException(
            "No gamma information available. The image has no sRGB or gAMA chunk.");
    }

    /// <summary>
    /// Returns a new <see cref="PlteChunkData"/> with gamma-corrected RGB entries.
    /// Returns null if the image has no palette.
    /// </summary>
    /// <param name="png">The PNG image</param>
    /// <param name="displayGamma">The display gamma exponent (default 2.2 for sRGB monitors)</param>
    public static PlteChunkData? GetGammaCorrectedPalette(this IRawPng png, double displayGamma = 2.2)
    {
        if (!png.Plte.HasValue)
            return null;
        if (displayGamma <= 0)
            throw new ArgumentOutOfRangeException(nameof(displayGamma), "Display gamma must be greater than zero.");

        var fileGamma = png.GetFileGamma()
            ?? throw new InvalidOperationException(
                "No gamma information available. The image has no sRGB or gAMA chunk.");

        var exponent = 1.0 / (fileGamma * displayGamma);
        var lut = GammaUtils.BuildLut(c => Math.Pow(c, exponent));
        return GammaUtils.CorrectPalette(png.Plte.Value, lut);
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

/// <summary>
/// Represents the dimensions of a PNG image
/// </summary>
/// <param name="Width">The width of the image in pixels</param>
/// <param name="Height">The height of the image in pixels</param>
public readonly record struct PngSize(int Width, int Height);