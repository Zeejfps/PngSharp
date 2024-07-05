using System.Text;

namespace PngSharp;

public sealed class PngReader
{
    private readonly Stream m_Stream;
    private readonly byte[] m_Buffer;

    public PngReader(Stream stream)
    {
        m_Stream = stream;
        m_Buffer = new byte[512];
    }
    
    public ReadOnlySpan<byte> ReadSignature()
    {
        return ReadBytesLittleEndian(8);
    }

    public PngSpec.IhdrChunkData ReadIhdrChunkData()
    {
        var width = ReadUInt32();
        var height = ReadUInt32();
        var bitDepth = ReadByte();
        var colorType = ReadByte();
        var compressionMethod = ReadByte();
        var filterMethod = ReadByte();
        var interlaceMethod = ReadByte();
        
        return new PngSpec.IhdrChunkData
        {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ColorType = (PngSpec.ColorType)colorType,
            CompressionMethod = (PngSpec.CompressionMethod)compressionMethod,
            FilterMethod = (PngSpec.FilterMethod)filterMethod,
            InterlaceMethod = (PngSpec.InterlaceMethod)interlaceMethod
        };
    }

    private void ReadCrc()
    {
        ReadBytesBigEndian(4);
    }

    private byte ReadByte()
    {
        // NOTE(Zee): Potential bug -1 is returned?
        return (byte)m_Stream.ReadByte();
    }

    public void BeginReadChunk(out PngSpec.ChunkHeader header)
    {
        var chunkSize = ReadUInt32();
        var chunkName = ReadAsciiString(4);
        header = new PngSpec.ChunkHeader
        {
            ChunkSizeInBytes = (int)chunkSize,
            Name = chunkName
        };
    }

    public PngSpec.SrgbChunkData ReadSrgbChunkData()
    {
        var renderingIntent = ReadByte();
        return new PngSpec.SrgbChunkData
        {
            RenderingIntent = (PngSpec.RenderingIntent)renderingIntent
        };
    }

    public void EndReadChunk()
    {
        ReadCrc();
    }

    private UInt32 ReadUInt32()
    {
        var buffer = ReadBytesBigEndian(4);
        return BitConverter.ToUInt32(buffer);
    }

    private string ReadAsciiString(int sizeInBytes)
    {
        var buffer = ReadBytesLittleEndian(sizeInBytes);
        return Encoding.ASCII.GetString(buffer);
    }

    private ReadOnlySpan<byte> ReadBytesLittleEndian(int byteCount)
    {
        var bytesRead = m_Stream.Read(m_Buffer, 0, byteCount);
        if (bytesRead != byteCount)
            throw new Exception($"Failed to read png. Read {bytesRead} bytes, expected {byteCount}");
        return m_Buffer.AsSpan(0, byteCount);
    }
    
    private ReadOnlySpan<byte> ReadBytesBigEndian(int byteCount)
    {
        var bytesRead = m_Stream.Read(m_Buffer, 0, byteCount);
        if (bytesRead != byteCount)
            throw new Exception($"Failed to read png. Read {bytesRead} bytes, expected {byteCount}");
        var buffer = m_Buffer.AsSpan(0, byteCount);
        buffer.Reverse();
        return buffer;
    }

    public void ReadIdatChunkDataIntoStream(PngSpec.ChunkHeader header, Stream stream)
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

    public void ReadIdatChunkData(byte[] buffer)
    {
        m_Stream.Read(buffer);
    }

    public PngSpec.GammaChunkData ReadGamaChunkData()
    {
        var value = ReadUInt32();
        return new PngSpec.GammaChunkData
        {
            Value = value
        };
    }

    public object ReadPhysChunkData()
    {
        ReadBytesBigEndian(9);
        return "asdf";
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