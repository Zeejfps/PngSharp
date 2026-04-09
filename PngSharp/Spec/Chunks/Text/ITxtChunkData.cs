using System.IO.Compression;
using System.Text;

namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// iTXt chunk: international UTF-8 text metadata with language tag.
/// Data contains raw bytes — UTF-8 text if uncompressed, deflate bytes if compressed.
/// </summary>
public readonly record struct ITxtChunkData
{
    public string Keyword { get; init; }
    public string LanguageTag { get; init; }
    public string TranslatedKeyword { get; init; }
    public bool IsCompressed { get; init; }
    public byte[] Data { get; init; }

    public string GetText()
    {
        if (!IsCompressed)
            return Encoding.UTF8.GetString(Data);

        using var compressedStream = new MemoryStream(Data);
        using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return Encoding.UTF8.GetString(resultStream.ToArray());
    }

    public static ITxtChunkData Create(ITxtContent content)
    {
        return new ITxtChunkData
        {
            Keyword = content.Keyword,
            LanguageTag = content.LanguageTag,
            TranslatedKeyword = content.TranslatedKeyword,
            IsCompressed = false,
            Data = Encoding.UTF8.GetBytes(content.Text),
        };
    }

    public static ITxtChunkData Create(ITxtCompressedContent content)
    {
        var raw = Encoding.UTF8.GetBytes(content.Text);
        using var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.Optimal, true))
        {
            zlibStream.Write(raw);
        }

        return new ITxtChunkData
        {
            Keyword = content.Keyword,
            LanguageTag = content.LanguageTag,
            TranslatedKeyword = content.TranslatedKeyword,
            IsCompressed = true,
            Data = compressedStream.ToArray(),
        };
    }
}
