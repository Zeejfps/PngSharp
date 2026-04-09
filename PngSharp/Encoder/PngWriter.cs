using System.Runtime.InteropServices;
using System.Text;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Encoder;

internal sealed class PngWriter : IDisposable, IAsyncDisposable
{
    private readonly Stream m_Stream;
    private readonly PngCrc32 m_Crc32;

    public PngWriter(Stream stream, PngCrc32 crc32)
    {
        m_Stream = stream;
        m_Crc32 = crc32;
    }

    public void WriteSignature()
    {
        m_Stream.Write(PngSpecUtils.PNG_SIGNATURE);
    }
    
    public void WriteIHDRChunk(IhdrChunkData data)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.IHDR,
            ChunkSizeInBytes = 13
        });
        WriteUInt32(data.Width);
        WriteUInt32(data.Height);
        WriteByte(data.BitDepth);
        WriteByte((byte)data.ColorType);
        WriteByte((byte)data.CompressionMethod);
        WriteByte((byte)CompressionMethod.DeflateWithSlidingWindow);
        WriteByte((byte)InterlaceMethod.None);
        WriteCrc32();
    }

    public void WriteIDATChunk(ReadOnlySpan<byte> data)
    {
        var sizeInBytes = data.Length;
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.IDAT,
            ChunkSizeInBytes = sizeInBytes
        });
        WriteBytes(data);
        WriteCrc32();
    }

    public void WriteSRGBChunk(SrgbChunkData srgbChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.SRGB,
            ChunkSizeInBytes = 1
        });
        WriteByte((byte)srgbChunkData.RenderingIntent);
        WriteCrc32();
    }

    public void WriteGAMAChunk(GammaChunkData gammaChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.GAMA,
            ChunkSizeInBytes = 4
        });
        WriteUInt32(gammaChunkData.Value);
        WriteCrc32();
    }

    public void WritePHYSChunk(PhysChunkData physChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.PHYS,
            ChunkSizeInBytes = 9
        });
        WriteUInt32(physChunkData.XAxisPPU);
        WriteUInt32(physChunkData.YAxisPPU);
        WriteByte((byte)physChunkData.UnitSpecifier);
        WriteCrc32();
    }

    public void WriteIENDChunk()
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.IEND,
            ChunkSizeInBytes = 0
        });
        WriteCrc32();
        m_Stream.Flush();
    }

    public void WriteCrc32()
    {
        var crc = m_Crc32.Value;
        WriteUInt32(crc);
    }

    public void WriteChunkHeader(ChunkHeader header)
    {
        WriteHeaderSize((uint)header.ChunkSizeInBytes);
        m_Crc32.Reset();
        WriteHeaderName(header.Id);
    }

    private void WriteHeaderName(string name)
    {
        Span<byte> buffer = stackalloc byte[4];
        Encoding.ASCII.GetBytes(name, buffer);
        WriteBytes(buffer);
    }

    private void WriteHeaderSize(uint size)
    {
        WriteUInt32(size);
    }

    private void WriteUInt32(uint value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        MemoryMarshal.Write(buffer, in value);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();
        WriteBytes(buffer);
    }

    private void WriteByte(byte b)
    {
        m_Stream.WriteByte(b);
        m_Crc32.Update(b);
    }

    private void WriteBytes(ReadOnlySpan<byte> data)
    {
        m_Stream.Write(data);
        m_Crc32.Update(data);
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