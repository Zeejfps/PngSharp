using System.Text;

namespace PngSharp.Encoder;

internal sealed class PngWriter
{
    private readonly Stream m_Stream;

    public PngWriter(Stream stream)
    {
        m_Stream = stream;
    }

    public void WriteSignature()
    {
        m_Stream.Write(PngSpec.PNG_SIGNATURE);
    }
    
    public void WriteIHDRChunk(PngSpec.IhdrChunkData data)
    {
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
    }

    private void WriteByte(byte b)
    {
        m_Stream.WriteByte(b);
    }

    public void WriteIDATChunk(ReadOnlySpan<byte> data)
    {
        var sizeInBytes = data.Length;
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IDAT,
            ChunkSizeInBytes = sizeInBytes
        });
        m_Stream.Write(data);
    }

    public void WriteIENDChunk()
    {
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IEND,
            ChunkSizeInBytes = 0
        });
        m_Stream.Flush();
    }

    private void WriteChunkHeader(PngSpec.ChunkHeader header)
    {
        WriteHeaderName(header.Name);
        WriteUInt32((uint)header.ChunkSizeInBytes);
    }

    private void WriteHeaderName(string name)
    {
        var valueBytes = Encoding.ASCII.GetBytes(name);
        m_Stream.Write(valueBytes);
    }

    private void WriteUInt32(uint value)
    {
        var bytes = BitConverter.GetBytes(value).AsSpan();
        // TODO: verify endines
        bytes.Reverse();
        m_Stream.Write(bytes);
    }
}