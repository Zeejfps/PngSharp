using PngSharp.Decoder;
using PngSharp.Encoder;
using PngSharp.Spec;

namespace PngSharp.Api;

public sealed class PngApi
{
    private readonly ILogger m_Logger;
    
    public PngApi() : this(new NullLogger())
    {
        
    }

    public PngApi(ILogger logger)
    {
        m_Logger = logger;
    }
    
    /// <summary>
    /// Decodes a PNG image from a file
    /// </summary>
    /// <param name="pathToFile"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public IDecodedPng DecodeFromFile(string pathToFile)
    {
        using var fileStream = new FileStream(pathToFile, FileMode.Open);
        return DecodeFromStream(fileStream);
    }
    
    /// <summary>
    /// Decodes a PNG image from a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns>Decoded PNG image containing information about the image and its pixel data</returns>
    public IDecodedPng DecodeFromStream(Stream stream)
    {
        var reader = new PngReader(stream);
        using var decoder = new PngDecoder(reader, m_Logger);
        decoder.Decode();
        var imageWidth = (int)decoder.IhdrChunkData.Width;
        var imageHeight = (int)decoder.IhdrChunkData.Height;
        var pixelData = new byte[imageWidth * imageHeight * decoder.BytesPerPixel];
        decoder.PixelDataStream.Position = 0;
        var pixelsRead = decoder.PixelDataStream.Read(pixelData);
        // TODO: verify pixelsRead matches?

        var decodedPng = decoder.DecodedPng;
        decodedPng.BytesPerPixel = decoder.BytesPerPixel;
        decodedPng.PixelData = pixelData;

        return decodedPng;
    }

    public void EncodeToFile(IDecodedPng decodedPng, string pathToFile)
    {
        using var fileStream = new FileStream(pathToFile, FileMode.Create);
        EncodeToStream(decodedPng, fileStream);
    }

    public void EncodeToStream(IDecodedPng decodedPng, Stream stream)
    {
        var crc32 = new PngCrc32();
        using var writer = new PngWriter(stream, crc32);
        using var encoder = new PngEncoder(decodedPng, writer, m_Logger);
        encoder.Encode();
    }

    private sealed class NullLogger : ILogger
    {
        public void Debug(string message) { }
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Error(string message) { }
    }
}