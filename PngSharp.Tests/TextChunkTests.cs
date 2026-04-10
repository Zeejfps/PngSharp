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
        var text = new TextChunk { Keyword = "Comment", Text = "Hello, PNG!" };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TxtChunks);
        Assert.Equal("Comment", decoded.TxtChunks[0].Keyword);
        Assert.Equal("Hello, PNG!", decoded.TxtChunks[0].Text);
    }

    [Fact]
    public void RoundTrip_tEXt_EmptyText_Preserved()
    {
        var text = new TextChunk { Keyword = "Comment", Text = "" };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TxtChunks);
        Assert.Equal("Comment", decoded.TxtChunks[0].Keyword);
        Assert.Equal("", decoded.TxtChunks[0].Text);
    }

    [Fact]
    public void RoundTrip_zTXt_CompressedDataPreserved()
    {
        var originalText = "This is a longer description that benefits from compression.";
        var chunk = ZTextChunk.Create(new ZTextContent
        {
            Keyword = "Description",
            Text = originalText,
        });

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithZTxtChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.ZTxtChunks);
        Assert.Equal("Description", decoded.ZTxtChunks[0].Keyword);

        var content = decoded.ZTxtChunks[0].DecodeContent();
        Assert.Equal(originalText, content.Text);
    }

    [Fact]
    public void RoundTrip_iTXt_Uncompressed_Preserved()
    {
        var chunk = ITextChunk.Create(new ITextContent
        {
            Keyword = "Title",
            Text = "PNG test image",
            LanguageTag = "en",
            TranslatedKeyword = "Title",
        });

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithITxtChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.ITxtChunks);
        var result = decoded.ITxtChunks[0];
        var content = result.DecodeContent();
        Assert.Equal("Title", content.Keyword);
        Assert.Equal("en", content.LanguageTag);
        Assert.Equal("Title", content.TranslatedKeyword);
        Assert.False(result.IsCompressed);
        Assert.Equal("PNG test image", content.Text);
    }

    [Fact]
    public void RoundTrip_iTXt_Compressed_Preserved()
    {
        var originalText = "A compressed international text chunk with UTF-8 support.";
        var chunk = ITextChunk.CreateCompressed(new ITextContent
        {
            Keyword = "Description",
            Text = originalText,
            LanguageTag = "en",
            TranslatedKeyword = "Description",
        });

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithITxtChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.ITxtChunks);
        var result = decoded.ITxtChunks[0];
        Assert.True(result.IsCompressed);
        Assert.Equal(originalText, result.DecodeContent().Text);
    }

    [Fact]
    public void RoundTrip_MultipleTextChunkTypes_AllPreserved()
    {
        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTxtChunk(new TextChunk { Keyword = "Title", Text = "Test" })
            .WithTxtChunk(new TextChunk { Keyword = "Author", Text = "PngSharp" })
            .WithZTxtChunk(ZTextChunk.Create(new ZTextContent
            {
                Keyword = "Description",
                Text = "Compressed text",
            }))
            .WithITxtChunk(
                ITextChunk.Create(new ITextContent
                {
                    Keyword = "Comment",
                    Text = "テスト",
                    LanguageTag = "ja",
                    TranslatedKeyword = "コメント",
                }))
            .Build();
        var decoded = RoundTrip(png);

        Assert.Equal(2, decoded.TxtChunks.Count);
        Assert.Single(decoded.ZTxtChunks);
        Assert.Single(decoded.ITxtChunks);
        Assert.Equal("Title", decoded.TxtChunks[0].Keyword);
        Assert.Equal("Author", decoded.TxtChunks[1].Keyword);
        Assert.Equal("Description", decoded.ZTxtChunks[0].Keyword);
        Assert.Equal("Comment", decoded.ITxtChunks[0].Keyword);
    }

    [Fact]
    public void Build_EmptyKeyword_Throws()
    {
        var text = new TextChunk { Keyword = "", Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTxtChunk(text).Build());
    }

    [Fact]
    public void Build_KeywordTooLong_Throws()
    {
        var text = new TextChunk { Keyword = new string('x', 80), Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTxtChunk(text).Build());
    }

    private static IRawPng RoundTrip(IRawPng png)
    {
        return Png.DecodeFromByteArray(Png.EncodeToByteArray(png));
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

    private static IRawPng CreatePngWithText(TextChunk text)
    {
        return Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTxtChunk(text)
            .Build();
    }
}
