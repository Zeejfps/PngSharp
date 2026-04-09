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
        var text = new TxtChunk { Keyword = "Comment", Text = "Hello, PNG!" };
        var png = CreatePngWithText(text);
        var decoded = RoundTrip(png);

        Assert.Single(decoded.TxtChunks);
        Assert.Equal("Comment", decoded.TxtChunks[0].Keyword);
        Assert.Equal("Hello, PNG!", decoded.TxtChunks[0].Text);
    }

    [Fact]
    public void RoundTrip_tEXt_EmptyText_Preserved()
    {
        var text = new TxtChunk { Keyword = "Comment", Text = "" };
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
        var chunk = ZTxtChunk.Create("Description", originalText);

        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithZTxtChunk(chunk)
            .Build();
        var decoded = RoundTrip(png);

        Assert.Single(decoded.ZTxtChunks);
        Assert.Equal("Description", decoded.ZTxtChunks[0].Keyword);

        var decompressed = decoded.ZTxtChunks[0].Decompress();
        Assert.Equal(originalText, decompressed);
    }

    [Fact]
    public void RoundTrip_iTXt_Uncompressed_Preserved()
    {
        var chunk = ITxtChunk.Create(new ITxtContent
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
        var chunk = ITxtChunk.CreateCompressed(new ITxtContent
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
        Assert.Equal(originalText, result.GetText());
    }

    [Fact]
    public void RoundTrip_MultipleTextChunkTypes_AllPreserved()
    {
        var png = Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTxtChunk(new TxtChunk { Keyword = "Title", Text = "Test" })
            .WithTxtChunk(new TxtChunk { Keyword = "Author", Text = "PngSharp" })
            .WithZTxtChunk(new ZTxtChunk
            {
                Keyword = "Description",
                CompressedData = ZTxtChunk.Create("Description", "Compressed text").CompressedData,
            })
            .WithITxtChunk(
                ITxtChunk.Create(new ITxtContent
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
        var text = new TxtChunk { Keyword = "", Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTxtChunk(text).Build());
    }

    [Fact]
    public void Build_KeywordTooLong_Throws()
    {
        var text = new TxtChunk { Keyword = new string('x', 80), Text = "test" };
        Assert.Throws<InvalidOperationException>(() =>
            Png.Builder().WithIhdr(MakeIhdr()).WithPixelData([0, 0, 0, 255])
                .WithTxtChunk(text).Build());
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

    private static IRawPng CreatePngWithText(TxtChunk text)
    {
        return Png.Builder()
            .WithIhdr(MakeIhdr())
            .WithPixelData([0, 0, 0, 255])
            .WithTxtChunk(text)
            .Build();
    }
}
