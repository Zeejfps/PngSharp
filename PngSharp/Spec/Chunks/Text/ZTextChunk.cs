using System.IO.Compression;
using System.Text;

namespace PngSharp.Spec.Chunks.Text;

/// <summary>
/// zTXt chunk: deflate-compressed Latin-1 text metadata.
/// CompressedData contains the raw deflate bytes.
/// </summary>
public readonly record struct ZTextChunk
{
    public string Keyword { get; init; }
    public byte[] CompressedData { get; init; }

    public ZTextContent GetContent()
    {
        using var compressedStream = new MemoryStream(CompressedData);
        using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return new ZTextContent
        {
            Keyword = Keyword,
            Text = Encoding.Latin1.GetString(resultStream.ToArray()),
        };
    }

    public static ZTextChunk Create(ZTextContent content)
    {
        var raw = Encoding.Latin1.GetBytes(content.Text);
        using var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.Optimal, true))
        {
            zlibStream.Write(raw);
        }
        return new ZTextChunk { Keyword = content.Keyword, CompressedData = compressedStream.ToArray() };
    }
}
