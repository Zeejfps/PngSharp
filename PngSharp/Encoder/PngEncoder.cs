using System.IO.Compression;

namespace PngSharp.Encoder;

internal sealed class PngEncoder
{
    private readonly IDecodedPng m_Png;
    private readonly PngWriter m_PngWriter;
    private readonly byte[] m_Buffer;

    public PngEncoder(IDecodedPng png)
    {
        m_Png = png;
        m_PngWriter = new PngWriter();
        m_Buffer = new byte[png.Width * png.BytesPerPixel];
    }
    
    public void Encode()
    {
        m_PngWriter.WriteSignature();
        m_PngWriter.WriteIHDRChunk(new PngSpec.IhdrChunkData
        {
            Width = (uint)m_Png.Width,
            Height = (uint)m_Png.Height,
        });

        using var memStream = new MemoryStream(m_Png.PixelData);
        using var compressionStream = new DeflateStream(memStream, CompressionMode.Compress);
        EncodePixels(compressionStream, memStream);
        using var compressedDataStream = new MemoryStream();
        compressionStream.CopyTo(compressedDataStream);
        m_PngWriter.WriteIDATChunk(compressedDataStream.ToArray());
        
        m_PngWriter.WriteIENDChunk();
    }
    
    private void EncodePixels(Stream outputStream, Stream inputStream)
    {
        var bytesRead = inputStream.Read(m_Buffer);
        outputStream.WriteByte((byte)PngSpec.AdaptiveFilteringType.None);
        outputStream.Write(m_Buffer);
    }
}