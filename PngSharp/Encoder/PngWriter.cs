using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.tIME;
using PngSharp.Spec.Chunks.tRNS;
using PngSharp.Spec.Chunks.sBIT;
using PngSharp.Spec.Chunks.iCCP;
using PngSharp.Spec.Chunks.eXIf;

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
        WriteByte((byte)data.FilterMethod);
        WriteByte((byte)data.InterlaceMethod);
        WriteCrc32();
    }

    public void WritePLTEChunk(PlteChunkData plteChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.PLTE,
            ChunkSizeInBytes = plteChunkData.Entries.Length
        });
        WriteBytes(plteChunkData.Entries);
        WriteCrc32();
    }

    public void WriteTRNSChunk(TrnsChunkData trnsChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.TRNS,
            ChunkSizeInBytes = trnsChunkData.Data.Length
        });
        WriteBytes(trnsChunkData.Data);
        WriteCrc32();
    }

    private const int MaxKeywordLength = 79;
    private const int MaxLanguageTagLength = 255;

    public void WriteTxtChunk(TextChunk textChunkData)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(textChunkData.Keyword.Length, MaxKeywordLength, nameof(textChunkData.Keyword));

        Span<byte> keyword = stackalloc byte[textChunkData.Keyword.Length];
        Encoding.Latin1.GetBytes(textChunkData.Keyword, keyword);
        var text = Encoding.Latin1.GetBytes(textChunkData.Text);
        var size = keyword.Length + 1 + text.Length;

        WriteChunkHeader(new ChunkHeader { Id = HeaderIds.TEXT, ChunkSizeInBytes = size });
        WriteBytes(keyword);
        WriteByte(0); // null separator
        WriteBytes(text);
        WriteCrc32();
    }

    public void WriteZTxtChunk(ZTextChunk chunk)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(chunk.Keyword.Length, MaxKeywordLength, nameof(chunk.Keyword));

        Span<byte> keyword = stackalloc byte[chunk.Keyword.Length];
        Encoding.Latin1.GetBytes(chunk.Keyword, keyword);

        var size = keyword.Length + 1 + 1 + chunk.CompressedData.Length;
        WriteChunkHeader(new ChunkHeader { Id = HeaderIds.ZTXT, ChunkSizeInBytes = size });
        WriteBytes(keyword);
        WriteByte(0); // null separator
        WriteByte(0); // compression method = deflate
        WriteBytes(chunk.CompressedData);
        WriteCrc32();
    }

    public void WriteITxtChunk(ITextChunk chunk)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(chunk.Keyword.Length, MaxKeywordLength, nameof(chunk.Keyword));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(chunk.LanguageTag.Length, MaxLanguageTagLength, nameof(chunk.LanguageTag));

        Span<byte> keyword = stackalloc byte[chunk.Keyword.Length];
        Encoding.Latin1.GetBytes(chunk.Keyword, keyword);

        Span<byte> languageTag = stackalloc byte[chunk.LanguageTag.Length];
        Encoding.ASCII.GetBytes(chunk.LanguageTag, languageTag);

        var translatedKeyword = Encoding.UTF8.GetBytes(chunk.TranslatedKeyword);

        var size = keyword.Length + 1 + 1 + 1 + languageTag.Length + 1 + translatedKeyword.Length + 1 + chunk.Data.Length;
        WriteChunkHeader(new ChunkHeader { Id = HeaderIds.ITXT, ChunkSizeInBytes = size });
        WriteBytes(keyword);
        WriteByte(0); // null separator
        WriteByte((byte)(chunk.IsCompressed ? 1 : 0)); // compression flag
        WriteByte(0); // compression method = deflate
        WriteBytes(languageTag);
        WriteByte(0); // null separator
        WriteBytes(translatedKeyword);
        WriteByte(0); // null separator
        WriteBytes(chunk.Data);
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

    public void WriteCHRMChunk(ChrmChunkData chrmChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.CHRM,
            ChunkSizeInBytes = 32
        });
        WriteUInt32(chrmChunkData.WhitePointX);
        WriteUInt32(chrmChunkData.WhitePointY);
        WriteUInt32(chrmChunkData.RedX);
        WriteUInt32(chrmChunkData.RedY);
        WriteUInt32(chrmChunkData.GreenX);
        WriteUInt32(chrmChunkData.GreenY);
        WriteUInt32(chrmChunkData.BlueX);
        WriteUInt32(chrmChunkData.BlueY);
        WriteCrc32();
    }

    public void WriteTIMEChunk(TimeChunkData timeChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.TIME,
            ChunkSizeInBytes = 7
        });
        WriteUInt16(timeChunkData.Year);
        WriteByte(timeChunkData.Month);
        WriteByte(timeChunkData.Day);
        WriteByte(timeChunkData.Hour);
        WriteByte(timeChunkData.Minute);
        WriteByte(timeChunkData.Second);
        WriteCrc32();
    }

    public void WriteBKGDChunk(BkgdChunkData bkgdChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.BKGD,
            ChunkSizeInBytes = bkgdChunkData.Data.Length
        });
        WriteBytes(bkgdChunkData.Data);
        WriteCrc32();
    }

    public void WriteSBITChunk(SbitChunkData sbitChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.SBIT,
            ChunkSizeInBytes = sbitChunkData.Data.Length
        });
        WriteBytes(sbitChunkData.Data);
        WriteCrc32();
    }

    public void WriteICCPChunk(IccpChunkData iccpChunkData)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            iccpChunkData.ProfileName.Length, MaxKeywordLength, nameof(iccpChunkData.ProfileName));

        Span<byte> profileName = stackalloc byte[iccpChunkData.ProfileName.Length];
        Encoding.Latin1.GetBytes(iccpChunkData.ProfileName, profileName);

        var size = profileName.Length + 1 + 1 + iccpChunkData.CompressedProfile.Length;
        WriteChunkHeader(new ChunkHeader { Id = HeaderIds.ICCP, ChunkSizeInBytes = size });
        WriteBytes(profileName);
        WriteByte(0); // null separator
        WriteByte(0); // compression method = deflate
        WriteBytes(iccpChunkData.CompressedProfile);
        WriteCrc32();
    }

    public void WriteEXIFChunk(ExifChunkData exifChunkData)
    {
        WriteChunkHeader(new ChunkHeader
        {
            Id = HeaderIds.EXIF,
            ChunkSizeInBytes = exifChunkData.Data.Length
        });
        WriteBytes(exifChunkData.Data);
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

    private void WriteUInt16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        MemoryMarshal.Write(buffer, in value);
        if (BitConverter.IsLittleEndian)
            buffer.Reverse();
        WriteBytes(buffer);
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