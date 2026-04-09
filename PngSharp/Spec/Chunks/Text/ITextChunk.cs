using System.IO.Compression;
using System.Text;

namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// iTXt chunk: international UTF-8 text metadata with language tag.
/// Data contains raw bytes — UTF-8 text if uncompressed, deflate bytes if compressed.
/// </summary>
public readonly record struct ITextChunk
{
    public string Keyword { get; init; }
    public string LanguageTag { get; init; }
    public string TranslatedKeyword { get; init; }
    public bool IsCompressed { get; init; }
    public byte[] Data { get; init; }

    public ITextContent DecodeContent()
    {
        string text;
        if (!IsCompressed)
        {
            text = Encoding.UTF8.GetString(Data);
        }
        else
        {
            using var compressedStream = new MemoryStream(Data);
            using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            deflateStream.CopyTo(resultStream);
            text = Encoding.UTF8.GetString(resultStream.ToArray());
        }

        return new ITextContent
        {
            Keyword = Keyword,
            Text = text,
            LanguageTag = LanguageTag,
            TranslatedKeyword = TranslatedKeyword,
        };
    }

    public static ITextChunk Create(ITextContent content)
    {
        return new ITextChunk
        {
            Keyword = content.Keyword,
            LanguageTag = content.LanguageTag,
            TranslatedKeyword = content.TranslatedKeyword,
            IsCompressed = false,
            Data = Encoding.UTF8.GetBytes(content.Text),
        };
    }

    public static ITextChunk CreateCompressed(ITextContent content)
    {
        var raw = Encoding.UTF8.GetBytes(content.Text);
        using var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.Optimal, true))
        {
            zlibStream.Write(raw);
        }

        return new ITextChunk
        {
            Keyword = content.Keyword,
            LanguageTag = content.LanguageTag,
            TranslatedKeyword = content.TranslatedKeyword,
            IsCompressed = true,
            Data = compressedStream.ToArray(),
        };
    }
}
