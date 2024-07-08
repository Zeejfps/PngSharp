using System.Text;

namespace PngSharp.Encoder;

internal sealed class PngWriter
{
    private readonly Stream m_Stream;
    private readonly PngCrcBuilder m_CrcBuilder;

    public PngWriter(Stream stream)
    {
        m_Stream = stream;
        m_CrcBuilder = new PngCrcBuilder();
    }

    public void WriteSignature()
    {
        m_Stream.Write(PngSpec.PNG_SIGNATURE);
    }
    
    public void WriteIHDRChunk(PngSpec.IhdrChunkData data)
    {
        m_CrcBuilder.Begin();
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IHDR,
            ChunkSizeInBytes = 13
        });
        WriteUInt32(data.Width);
        WriteUInt32(data.Height);
        WriteByte(data.BitDepth);
        WriteByte((byte)data.ColorType);
        WriteByte((byte)data.CompressionMethod);
        WriteByte((byte)PngSpec.CompressionMethod.DeflateWithSlidingWindow);
        WriteByte((byte)PngSpec.InterlaceMethod.None);
        
        var crc = m_CrcBuilder.End();
        WriteUInt32(crc);
    }

    public void WriteIDATChunk(ReadOnlySpan<byte> data)
    {
        m_CrcBuilder.Begin();
        var sizeInBytes = data.Length;
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IDAT,
            ChunkSizeInBytes = sizeInBytes
        });
        m_Stream.Write(data);
        m_CrcBuilder.Update(data);
        
        var crc = m_CrcBuilder.End();
        WriteUInt32(crc);
    }

    public void WriteIENDChunk()
    {
        m_CrcBuilder.Begin();
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IEND,
            ChunkSizeInBytes = 0
        });
        var crc = m_CrcBuilder.End();
        WriteUInt32(crc);
        m_Stream.Flush();
    }

    private void WriteChunkHeader(PngSpec.ChunkHeader header)
    {
        WriteUInt32((uint)header.ChunkSizeInBytes);
        WriteHeaderName(header.Name);
    }

    private void WriteByte(byte b)
    {
        m_Stream.WriteByte(b);
        m_CrcBuilder.Update(b);
    }

    private void WriteHeaderName(string name)
    {
        var valueBytes = Encoding.ASCII.GetBytes(name).AsSpan();
        m_Stream.Write(valueBytes);
        m_CrcBuilder.Update(valueBytes);
    }

    private void WriteUInt32(uint value)
    {
        var bytes = BitConverter.GetBytes(value).AsSpan();
        // TODO: verify endines
        bytes.Reverse();
        m_Stream.Write(bytes);
        m_CrcBuilder.Update(bytes);
    }
}