using System.Text;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp;

public sealed class PngReader
{
    private readonly Stream m_Stream;
    private readonly byte[] m_Buffer;
    private readonly PngCrc32 m_Crc32;

    public PngReader(Stream stream)
    {
        m_Stream = stream;
        m_Buffer = new byte[512];
        m_Crc32 = new PngCrc32();
    }

    public uint CurrentCrcValue => m_Crc32.Value;

    public ReadOnlySpan<byte> ReadSignature()
    {
        return ReadBytes(8);
    }

    public IhdrChunkData ReadIhdrChunkData()
    {
        var width = ReadUInt32();
        var height = ReadUInt32();
        var bitDepth = ReadByte();
        var colorType = ReadByte();
        var compressionMethod = ReadByte();
        var filterMethod = ReadByte();
        var interlaceMethod = ReadByte();
        
        return new IhdrChunkData
        {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ColorType = (ColorType)colorType,
            CompressionMethod = (CompressionMethod)compressionMethod,
            FilterMethod = (FilterMethod)filterMethod,
            InterlaceMethod = (InterlaceMethod)interlaceMethod
        };
    }

    private byte ReadByte()
    {
        // NOTE(Zee): Potential bug -1 is returned?
        var b = (byte)m_Stream.ReadByte();
        m_Crc32.Update(b);
        return b;
    }

    public void ReadChunkHeader(out ChunkHeader header)
    {
        var chunkSize = ReadUInt32();
        m_Crc32.Reset();
        var chunkName = ReadAsciiString(4);
        header = new ChunkHeader
        {
            ChunkSizeInBytes = (int)chunkSize,
            Name = chunkName
        };
    }

    public SrgbChunkData ReadSrgbChunkData()
    {
        var renderingIntent = ReadByte();
        return new SrgbChunkData
        {
            RenderingIntent = (RenderingIntent)renderingIntent
        };
    }

    public uint ReadCrc()
    {
        return ReadUInt32();
    }

    private UInt32 ReadUInt32()
    {
        var buffer = ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();
        
        return BitConverter.ToUInt32(buffer);
    }

    private string ReadAsciiString(int sizeInBytes)
    {
        var buffer = ReadBytes(sizeInBytes);
        return Encoding.ASCII.GetString(buffer);
    }

    private Span<byte> ReadBytes(int byteCount)
    {
        var bytesRead = m_Stream.Read(m_Buffer, 0, byteCount);
        if (bytesRead != byteCount)
            throw new Exception($"Failed to read png. Read {bytesRead} bytes, expected {byteCount}");
        var buffer = m_Buffer.AsSpan(0, byteCount);
        m_Crc32.Update(buffer);
        return buffer;
    }

    public void ReadIdatChunkDataIntoStream(ChunkHeader header, Stream stream)
    {
        var chunkSize = (int)header.ChunkSizeInBytes;
        var remainingBytesToRead = chunkSize;
        while (remainingBytesToRead > 0)
        {
            var bytesToRead = m_Buffer.Length < remainingBytesToRead ? m_Buffer.Length : remainingBytesToRead;
            var bytesRead = m_Stream.Read(m_Buffer, 0, bytesToRead);
            stream.Write(m_Buffer, 0, bytesRead);
            remainingBytesToRead -= bytesRead;
        }
    }

    public GammaChunkData ReadGamaChunkData()
    {
        var value = ReadUInt32();
        return new GammaChunkData
        {
            Value = value
        };
    }

    public PhysChunkData ReadPhysChunkData()
    {
        var xAxisPpu = ReadUInt32();
        var yAxisPpu = ReadUInt32();
        var unitSpecifier = ReadByte();
        return new PhysChunkData
        {
            XAxisPPU = xAxisPpu,
            YAxisPPU = yAxisPpu,
            UnitSpecifier = (UnitSpecifier)unitSpecifier
        };
    }

    public void ReadChunkData(int totalBytesToRead)
    {
        while (totalBytesToRead > 0)
        {
            var bytesToRead = m_Buffer.Length < totalBytesToRead ? m_Buffer.Length : totalBytesToRead;
            var bytesRead = m_Stream.Read(m_Buffer, 0, bytesToRead);
            totalBytesToRead -= bytesRead;
        }
    }
}