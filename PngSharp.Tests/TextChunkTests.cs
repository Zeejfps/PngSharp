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
        Assert.False(decoded.TextChunks[0].IsCompressed);
        Assert.False(decoded.TextChunks[0].IsInternational);
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
    public void RoundTrip_zTXt_Preserved()
    {
        var text = new TextChunkData
        {
            Keyword = "Description",
            Text = "This is a longer description that benefits from compression.",
            IsCompressed = true,
        };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TextChunks);
        Assert.Equal("Description", decoded.TextChunks[0].Keyword);
        Assert.Equal(text.Text, decoded.TextChunks[0].Text);
        Assert.True(decoded.TextChunks[0].IsCompressed);
        Assert.False(decoded.TextChunks[0].IsInternational);
    }

    [Fact]
    public void RoundTrip_iTXt_Uncompressed_Preserved()
    {
        var text = new TextChunkData
        {
            Keyword = "Title",
            Text = "PNG test image",
            LanguageTag = "en",
            TranslatedKeyword = "Title",
        };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TextChunks);
        Assert.Equal("Title", decoded.TextChunks[0].Keyword);
        Assert.Equal("PNG test image", decoded.TextChunks[0].Text);
        Assert.Equal("en", decoded.TextChunks[0].LanguageTag);
        Assert.Equal("Title", decoded.TextChunks[0].TranslatedKeyword);
        Assert.False(decoded.TextChunks[0].IsCompressed);
        Assert.True(decoded.TextChunks[0].IsInternational);
    }

    [Fact]
    public void RoundTrip_iTXt_Compressed_Preserved()
    {
        var text = new TextChunkData
        {
            Keyword = "Description",
            Text = "A compressed international text chunk with UTF-8 support.",
            IsCompressed = true,
            LanguageTag = "en",
            TranslatedKeyword = "Description",
        };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TextChunks);
        Assert.Equal(text.Text, decoded.TextChunks[0].Text);
        Assert.True(decoded.TextChunks[0].IsCompressed);
        Assert.True(decoded.TextChunks[0].IsInternational);
    }

    [Fact]
    public void RoundTrip_MultipleTextChunks_AllPreserved()
    {
        var chunks = new[]
        {
            new TextChunkData { Keyword = "Title", Text = "Test Image" },
            new TextChunkData { Keyword = "Author", Text = "PngSharp" },
            new TextChunkData { Keyword = "Software", Text = "PngSharp Tests" },
        };

        var builder = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255]);
        foreach (var chunk in chunks)
            builder.WithTextChunk(chunk);
        var png = builder.Build();
        var decoded = RoundTrip(png);

        Assert.Equal(3, decoded.TextChunks.Count);
        Assert.Equal("Title", decoded.TextChunks[0].Keyword);
        Assert.Equal("Author", decoded.TextChunks[1].Keyword);
        Assert.Equal("Software", decoded.TextChunks[2].Keyword);
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
