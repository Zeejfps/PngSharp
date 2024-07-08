using System.Text;

namespace PngSharp.Encoder;

internal sealed class PngWriter : IDisposable, IAsyncDisposable
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
        
        WriteCrc32();
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
        WriteBytes(data);
        WriteCrc32();
    }

    public void WriteIENDChunk()
    {
        m_CrcBuilder.Begin();
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IEND,
            ChunkSizeInBytes = 0
        });
        WriteCrc32();
        m_Stream.Flush();
    }

    private void WriteCrc32()
    {
        var crc = m_CrcBuilder.End();
        WriteUInt32(crc);
    }
    
    private void WriteChunkHeader(PngSpec.ChunkHeader header)
    {
        WriteUInt32((uint)header.ChunkSizeInBytes);
        WriteHeaderName(header.Name);
    }

    private void WriteHeaderName(string name)
    {
        var valueBytes = Encoding.ASCII.GetBytes(name).AsSpan();
        WriteBytes(valueBytes);
    }

    private void WriteUInt32(uint value)
    {
        var bytes = BitConverter.GetBytes(value).AsSpan();
        // TODO: verify endines
        bytes.Reverse();
        WriteBytes(bytes);
    }
    
    private void WriteByte(byte b)
    {
        m_Stream.WriteByte(b);
        m_CrcBuilder.Update(b);
    }
    
    private void WriteBytes(ReadOnlySpan<byte> data)
    {
        m_Stream.Write(data);
        m_CrcBuilder.Update(data);
    }

    public void Dispose()
    {
        m_Stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await m_Stream.DisposeAsync();
    }
}