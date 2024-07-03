using System.Text;

namespace PngSharp;

public sealed class PngReader
{
    public static PngReader ReadFromFile(string pathToFile)
    {
        var fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
        return new PngReader(fileStream);
    }

    private readonly Stream m_Stream;
    private readonly byte[] m_Buffer;

    private PngReader(Stream stream)
    {
        m_Stream = stream;
        m_Buffer = new byte[512];
    }
    
    public ReadOnlySpan<byte> ReadSig()
    {
        return ReadBytesLittleEndian(8);
    }

    public PngSpec.ImageData ReadIhdrChunkData()
    {
        var width = ReadUInt32();
        var height = ReadUInt32();
        var bitDepth = ReadByte();
        var colorType = ReadByte();
        var compressionMethod = ReadByte();
        var filterMethod = ReadByte();
        var interlaceMethod = ReadByte();
        
        return new PngSpec.ImageData
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

    public bool BeginReadChunk(out PngSpec.ChunkHeader header)
    {
        var chunkSize = ReadUInt32();
        var chunkName = ReadAsciiString(4);
        header = new PngSpec.ChunkHeader
        {
            ChunkSizeInBytes = chunkSize,
            Name = chunkName
        };
        return true;
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

    private Int32 ReadInt32()
    {
        var buffer = ReadBytesBigEndian(4);
        return BitConverter.ToInt32(buffer);
    }
    
    private UInt32 ReadUInt32()
    {
        var buffer = ReadBytesBigEndian(4);
        return BitConverter.ToUInt32(buffer);
    }

    private string ReadAsciiString(int sizeInBytes)
    {
        var buffer = ReadBytesLittleEndian(4);
        return Encoding.ASCII.GetString(buffer);
    }

    private ReadOnlySpan<byte> ReadBytesLittleEndian(int byteCount)
    {
        var bytesRead = m_Stream.Read(m_Buffer, 0, byteCount);
        if (bytesRead != byteCount)
            throw new Exception($"Failed to read signature. Read {bytesRead} bytes, expected {byteCount}");
        return m_Buffer.AsSpan(0, byteCount);
    }
    
    private ReadOnlySpan<byte> ReadBytesBigEndian(int byteCount)
    {
        var bytesRead = m_Stream.Read(m_Buffer, 0, byteCount);
        if (bytesRead != byteCount)
            throw new Exception($"Failed to read signature. Read {bytesRead} bytes, expected {byteCount}");
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
}