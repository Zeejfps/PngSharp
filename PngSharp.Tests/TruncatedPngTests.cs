using System.Buffers.Binary;
using System.Text;
using PngSharp.Api;
using PngSharp.Api.Exceptions;
using Xunit;

namespace PngSharp.Tests;

// A truncated file must fail with a PngFormatException. Before the fix, a file ending inside a
// chunk read by a Stream.Read loop (IDAT, unknown ancillary chunks) never finished decoding.
public class TruncatedPngTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    private static byte[] ValidPng()
    {
        var pixels = new byte[16 * 16 * 4];
        new Random(42).NextBytes(pixels);
        return Png.EncodeToByteArray(Png.CreateRgba(16, 16, pixels));
    }

    // Runs the decode on another thread, so a regression fails the test instead of hanging the run
    private static Exception? DecodeWithTimeout(byte[] data)
    {
        var decode = Task.Run(() => Record.Exception(() => Png.DecodeFromByteArray(data)));
        Assert.True(decode.Wait(Timeout), $"Decoding {data.Length} bytes did not finish within {Timeout}");
        return decode.Result;
    }

    [Fact]
    public void Decode_TruncatedInsideIdat_ThrowsPngFormatException()
    {
        var png = ValidPng();

        var ex = DecodeWithTimeout(png[..(png.Length - 20)]); // drops IEND and the end of the IDAT chunk

        Assert.IsType<PngFormatException>(ex);
    }

    [Fact]
    public void Decode_TruncatedInsideUnknownAncillaryChunk_ThrowsPngFormatException()
    {
        var png = ValidPng();
        const int ihdrEnd = 8 + 4 + 4 + 13 + 4; // signature, then IHDR's length, type, data and CRC
        var chunk = new byte[4 + 4 + 100];
        BinaryPrimitives.WriteUInt32BigEndian(chunk, 100);
        Encoding.ASCII.GetBytes("abCd").CopyTo(chunk, 4); // lowercase first letter: ancillary, so it is skipped
        byte[] truncated = [.. png[..ihdrEnd], .. chunk[..50]];

        var ex = DecodeWithTimeout(truncated);

        Assert.IsType<PngFormatException>(ex);
    }

    [Fact]
    public void Decode_EveryTruncatedPrefix_ThrowsPngFormatException()
    {
        var png = ValidPng();

        for (var length = 0; length < png.Length; length++)
        {
            var ex = DecodeWithTimeout(png[..length]);
            Assert.True(ex is PngFormatException,
                $"Prefix of {length}/{png.Length} bytes: expected PngFormatException, got {ex?.GetType().Name ?? "no exception"}");
        }
    }
}
