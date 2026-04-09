using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.Text;
using Xunit;

namespace PngSharp.Tests;

public class TextChunkTests
{
    [Fact]
    public void RoundTrip_tEXt_Preserved()
    {
        var text = new TextChunkData { Keyword = "Comment", Text = "Hello, PNG!" };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TextChunks);
        Assert.Equal("Comment", decoded.TextChunks[0].Keyword);
        Assert.Equal("Hello, PNG!", decoded.TextChunks[0].Text);
    }

    [Fact]
    public void RoundTrip_tEXt_EmptyText_Preserved()
    {
        var text = new TextChunkData { Keyword = "Comment", Text = "" };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TextChunks);
        Assert.Equal("Comment", decoded.TextChunks[0].Keyword);
        Assert.Equal("", decoded.TextChunks[0].Text);
    }

    [Fact]
    public void RoundTrip_zTXt_CompressedDataPreserved()
    {
        var originalText = "This is a longer description that benefits from compression.";
        var chunk = CompressedTextChunkData.Create("Description", originalText);

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithCompressedTextChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.CompressedTextChunks);
        Assert.Equal("Description", decoded.CompressedTextChunks[0].Keyword);

        var decompressed = decoded.CompressedTextChunks[0].Decompress();
        Assert.Equal(originalText, decompressed);
    }

    [Fact]
    public void RoundTrip_iTXt_Uncompressed_Preserved()
    {
        var chunk = InternationalTextChunkData.Create("Title", "PNG test image", "en", "Title");

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithInternationalTextChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.InternationalTextChunks);
        var result = decoded.InternationalTextChunks[0];
        Assert.Equal("Title", result.Keyword);
        Assert.Equal("en", result.LanguageTag);
        Assert.Equal("Title", result.TranslatedKeyword);
        Assert.False(result.IsCompressed);
        Assert.Equal("PNG test image", result.GetText());
    }

    [Fact]
    public void RoundTrip_iTXt_Compressed_Preserved()
    {
        var originalText = "A compressed international text chunk with UTF-8 support.";
        var chunk = InternationalTextChunkData.CreateCompressed("Description", originalText, "en", "Description");

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithInternationalTextChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.InternationalTextChunks);
        var result = decoded.InternationalTextChunks[0];
        Assert.True(result.IsCompressed);
        Assert.Equal(originalText, result.GetText());
    }

    [Fact]
    public void RoundTrip_MultipleTextChunkTypes_AllPreserved()
    {
        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTextChunk(new TextChunkData { Keyword = "Title", Text = "Test" })
            .WithTextChunk(new TextChunkData { Keyword = "Author", Text = "PngSharp" })
            .WithCompressedTextChunk(new CompressedTextChunkData
            {
                Keyword = "Description",
                CompressedData = CompressedTextChunkData.Create("Description", "Compressed text").CompressedData,
            })
            .WithInternationalTextChunk(
                InternationalTextChunkData.Create("Comment", "テスト", "ja", "コメント"))
            .Build();
        var decoded = RoundTrip(png);

        Assert.Equal(2, decoded.TextChunks.Count);
        Assert.Single(decoded.CompressedTextChunks);
        Assert.Single(decoded.InternationalTextChunks);
        Assert.Equal("Title", decoded.TextChunks[0].Keyword);
        Assert.Equal("Author", decoded.TextChunks[1].Keyword);
        Assert.Equal("Description", decoded.CompressedTextChunks[0].Keyword);
        Assert.Equal("Comment", decoded.InternationalTextChunks[0].Keyword);
    }

    [Fact]
    public void Build_EmptyKeyword_Throws()
    {
        var text = new TextChunkData { Keyword = "", Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTextChunk(text).Build());
    }

    [Fact]
    public void Build_KeywordTooLong_Throws()
    {
        var text = new TextChunkData { Keyword = new string('x', 80), Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTextChunk(text).Build());
    }

    private static IRawPng RoundTrip(IRawPng png)
    {
        var ms = new MemoryStream();
        Png.EncodeToStream(png, ms);
        return Png.DecodeFromByteArray(ms.ToArray());
    }

    private static IhdrChunkData MakeIhdr()
    {
        return new IhdrChunkData
        {
            Width = 1, Height = 1, BitDepth = 8,
            ColorType = ColorType.TrueColorWithAlpha,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }

    private static IRawPng CreatePngWithText(TextChunkData text)
    {
        return Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTextChunk(text)
            .Build();
    }
}
