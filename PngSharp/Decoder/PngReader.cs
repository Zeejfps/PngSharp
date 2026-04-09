using System.Text;
using PngSharp.Api.Exceptions;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.tRNS;

namespace PngSharp.Decoder;

internal sealed class PngReader
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
        var buffer = m_Buffer.AsSpan(0, 8);
        ReadBytes(buffer);
        return buffer;
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
        var chunkName = ReadChunkHeaderId();
        header = new ChunkHeader
        {
            ChunkSizeInBytes = (int)chunkSize,
            Id = chunkName
        };
    }

    public PlteChunkData ReadPlteChunkData(int chunkSize)
    {
        if (chunkSize == 0 || chunkSize % 3 != 0)
            throw new PngFormatException($"PLTE chunk data length {chunkSize} must be a positive multiple of 3.");

        var entries = new byte[chunkSize];
        ReadBytes(entries);
        return new PlteChunkData { Entries = entries };
    }

    public TrnsChunkData ReadTrnsChunkData(int chunkSize)
    {
        var data = new byte[chunkSize];
        ReadBytes(data);
        return new TrnsChunkData { Data = data };
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

    public void ReadAndValidateCrc(string chunkId)
    {
        var computed = CurrentCrcValue;
        var expected = ReadCrc();
        if (computed != expected)
            throw new PngCorruptException(chunkId, computed, expected);
    }

    private UInt32 ReadUInt32()
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        ReadBytes(buffer);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();
        return BitConverter.ToUInt32(buffer);
    }

    private string ReadChunkHeaderId()
    {
        Span<byte> buffer = stackalloc byte[4];
        ReadBytes(buffer);
        return Encoding.ASCII.GetString(buffer);
    }

    public void ReadBytes(Span<byte> buffer)
    {
        m_Stream.ReadExactly(buffer);
        m_Crc32.Update(buffer);
    }

    public void ReadIdatChunkDataIntoStream(ChunkHeader header, Stream stream)
    {
        var chunkSize = header.ChunkSizeInBytes;
        var remainingBytesToRead = chunkSize;
        while (remainingBytesToRead > 0)
        {
            var bytesToRead = m_Buffer.Length < remainingBytesToRead ? m_Buffer.Length : remainingBytesToRead;
            var bytesRead = m_Stream.Read(m_Buffer, 0, bytesToRead);
            m_Crc32.Update(m_Buffer.AsSpan(0, bytesRead));
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
            m_Crc32.Update(m_Buffer.AsSpan(0, bytesRead));
            totalBytesToRead -= bytesRead;
        }
    }
}