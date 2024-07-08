using System.IO.Compression;

namespace PngSharp.Encoder;

internal sealed class PngEncoder
{
    private readonly IDecodedPng m_Png;
    private readonly PngWriter m_PngWriter;
    private readonly byte[] m_Buffer;

    public PngEncoder(IDecodedPng png, Stream stream)
    {
        m_Png = png;
        m_PngWriter = new PngWriter(stream);
        m_Buffer = new byte[png.Width * png.BytesPerPixel];
    }
    
    public void Encode()
    {
        var png = m_Png;
        var writer = m_PngWriter;
        
        writer.WriteSignature();
        writer.WriteIHDRChunk(new PngSpec.IhdrChunkData
        {
            Width = (uint)png.Width,
            Height = (uint)png.Height,
            CompressionMethod = PngSpec.CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = PngSpec.FilterMethod.AdaptiveFiltering,
            ColorType = PixelFormatToColorType(png.PixelFormat),
            InterlaceMethod = PngSpec.InterlaceMethod.None, // TODO: Make dynamic?
            BitDepth = 8, // TODO: Make dynamic
        });
        //
        using var pixelDataStream = new MemoryStream(png.PixelData);
        using var compressedDataStream = new MemoryStream();
        using var compressionStream = new DeflateStream(compressedDataStream, CompressionMode.Compress);
        EncodePixels(compressionStream, pixelDataStream);
        writer.WriteIDATChunk(compressedDataStream.ToArray());
        
        writer.WriteIENDChunk();
    }
    
    private void EncodePixels(Stream outputStream, Stream inputStream)
    {
        var bytesRead = inputStream.Read(m_Buffer);
        outputStream.WriteByte((byte)PngSpec.AdaptiveFilteringType.None);
        outputStream.Write(m_Buffer);
    }

    private PngSpec.ColorType PixelFormatToColorType(PixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            PixelFormat.RGBA => PngSpec.ColorType.TrueColorWithAlpha,
            PixelFormat.RGB => PngSpec.ColorType.TrueColor,
            PixelFormat.Grayscale => PngSpec.ColorType.Grayscale,
            PixelFormat.GrayscaleWithAlpha => PngSpec.ColorType.GrayscaleWithAlpha,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null)
        };
    }
}