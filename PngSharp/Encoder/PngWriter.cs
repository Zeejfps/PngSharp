using System.Text;
using PngSharp.Common;

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
        var sizeInBytes = data.Length;
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.IDAT,
            ChunkSizeInBytes = sizeInBytes
        });
        WriteBytes(data);
        WriteCrc32();
    }

    public void WriteSRGBChunk(PngSpec.SrgbChunkData srgbChunkData)
    {
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.SRGB,
            ChunkSizeInBytes = 1
        });
        WriteByte((byte)srgbChunkData.RenderingIntent);
        WriteCrc32();
    }

    public void WriteGAMAChunk(PngSpec.GammaChunkData gammaChunkData)
    {
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.GAMA,
            ChunkSizeInBytes = 4
        });
        WriteUInt32(gammaChunkData.Value);
        WriteCrc32();
    }

    public void WritePHYSChunk(PngSpec.PhysChunkData physChunkData)
    {
        WriteChunkHeader(new PngSpec.ChunkHeader
        {
            Name = PngSpec.HeaderNames.PHYS,
            ChunkSizeInBytes = 9
        });
        WriteUInt32(physChunkData.XAxisPPU);
        WriteUInt32(physChunkData.YAxisPPU);
        WriteByte((byte)physChunkData.UnitSpecifier);
        WriteCrc32();
    }

    public void WriteIENDChunk()
    {
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
        WriteHeaderSize((uint)header.ChunkSizeInBytes);
        m_CrcBuilder.Begin();
        WriteHeaderName(header.Name);
    }

    private void WriteHeaderName(string name)
    {
        var valueBytes = Encoding.ASCII.GetBytes(name).AsSpan();
        WriteBytes(valueBytes);
    }

    private void WriteHeaderSize(uint size)
    {
        WriteUInt32(size);
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