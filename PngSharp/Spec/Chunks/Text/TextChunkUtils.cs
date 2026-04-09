using System.IO.Compression;
using System.Text;

namespace PngSharp.Spec.Chunks.Text;

public static class TextChunkUtils
{
    public static string Decompress(CompressedTextChunkData chunk)
    {
        using var compressedStream = new MemoryStream(chunk.CompressedData);
        using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return Encoding.Latin1.GetString(resultStream.ToArray());
    }

    public static string GetText(InternationalTextChunkData chunk)
    {
        if (!chunk.IsCompressed)
            return Encoding.UTF8.GetString(chunk.Data);

        using var compressedStream = new MemoryStream(chunk.Data);
        using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return Encoding.UTF8.GetString(resultStream.ToArray());
    }

    public static byte[] Compress(string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.Latin1;
        var raw = encoding.GetBytes(text);
        using var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.Optimal, true))
        {
            zlibStream.Write(raw);
        }
        return compressedStream.ToArray();
    }
}
