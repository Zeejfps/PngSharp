using PngSharp.Spec;
using Xunit;

namespace PngSharp.Tests;

public class BitDepthConverterTests
{
    // --- UnpackScanline tests ---

    [Fact]
    public void UnpackScanline_1Bit_8Pixels_ProducesCorrectSamples()
    {
        // 0b10110100 = 0xB4 -> [1, 0, 1, 1, 0, 1, 0, 0]
        ReadOnlySpan<byte> packed = [0xB4];
        Span<byte> output = new byte[8];
        BitDepthConverter.UnpackScanline(packed, output, 1, 8);
        Assert.Equal([1, 0, 1, 1, 0, 1, 0, 0], output.ToArray());
    }

    [Fact]
    public void UnpackScanline_1Bit_3Pixels_IgnoresPaddingBits()
    {
        // 0b10100000 = 0xA0 -> [1, 0, 1] (5 padding bits ignored)
        ReadOnlySpan<byte> packed = [0xA0];
        Span<byte> output = new byte[3];
        BitDepthConverter.UnpackScanline(packed, output, 1, 3);
        Assert.Equal([1, 0, 1], output.ToArray());
    }

    [Fact]
    public void UnpackScanline_2Bit_4Pixels_ProducesCorrectSamples()
    {
        // 0b11_10_01_00 = 0xE4 -> [3, 2, 1, 0]
        ReadOnlySpan<byte> packed = [0xE4];
        Span<byte> output = new byte[4];
        BitDepthConverter.UnpackScanline(packed, output, 2, 4);
        Assert.Equal([3, 2, 1, 0], output.ToArray());
    }

    [Fact]
    public void UnpackScanline_2Bit_3Pixels_IgnoresPaddingBits()
    {
        // 0b11_10_01_00 = 0xE4 -> [3, 2, 1] (last 2 bits ignored)
        ReadOnlySpan<byte> packed = [0xE4];
        Span<byte> output = new byte[3];
        BitDepthConverter.UnpackScanline(packed, output, 2, 3);
        Assert.Equal([3, 2, 1], output.ToArray());
    }

    [Fact]
    public void UnpackScanline_4Bit_2Pixels_ProducesCorrectSamples()
    {
        // 0xA5 -> [10, 5]
        ReadOnlySpan<byte> packed = [0xA5];
        Span<byte> output = new byte[2];
        BitDepthConverter.UnpackScanline(packed, output, 4, 2);
        Assert.Equal([10, 5], output.ToArray());
    }

    [Fact]
    public void UnpackScanline_4Bit_3Pixels_SpansTwoBytes()
    {
        // [0xA5, 0xB0] -> [10, 5, 11] (last 4 bits of second byte ignored)
        ReadOnlySpan<byte> packed = [0xA5, 0xB0];
        Span<byte> output = new byte[3];
        BitDepthConverter.UnpackScanline(packed, output, 4, 3);
        Assert.Equal([10, 5, 11], output.ToArray());
    }

    // --- PackScanline tests ---

    [Fact]
    public void PackScanline_1Bit_8Pixels_ProducesCorrectByte()
    {
        ReadOnlySpan<byte> unpacked = [1, 0, 1, 1, 0, 1, 0, 0];
        Span<byte> output = new byte[1];
        BitDepthConverter.PackScanline(unpacked, output, 1, 8);
        Assert.Equal([0xB4], output.ToArray());
    }

    [Fact]
    public void PackScanline_1Bit_3Pixels_PadsWithZeros()
    {
        ReadOnlySpan<byte> unpacked = [1, 0, 1];
        Span<byte> output = new byte[1];
        BitDepthConverter.PackScanline(unpacked, output, 1, 3);
        Assert.Equal([0xA0], output.ToArray());
    }

    [Fact]
    public void PackScanline_2Bit_4Pixels_ProducesCorrectByte()
    {
        ReadOnlySpan<byte> unpacked = [3, 2, 1, 0];
        Span<byte> output = new byte[1];
        BitDepthConverter.PackScanline(unpacked, output, 2, 4);
        Assert.Equal([0xE4], output.ToArray());
    }

    [Fact]
    public void PackScanline_4Bit_2Pixels_ProducesCorrectByte()
    {
        ReadOnlySpan<byte> unpacked = [10, 5];
        Span<byte> output = new byte[1];
        BitDepthConverter.PackScanline(unpacked, output, 4, 2);
        Assert.Equal([0xA5], output.ToArray());
    }

    [Fact]
    public void PackScanline_4Bit_3Pixels_SpansTwoBytes()
    {
        ReadOnlySpan<byte> unpacked = [10, 5, 11];
        Span<byte> output = new byte[2];
        BitDepthConverter.PackScanline(unpacked, output, 4, 3);
        Assert.Equal([0xA5, 0xB0], output.ToArray());
    }

    // --- Round-trip tests ---

    [Theory]
    [InlineData(1, 8, new byte[] { 1, 0, 1, 1, 0, 1, 0, 0 })]
    [InlineData(1, 3, new byte[] { 1, 0, 1 })]
    [InlineData(2, 4, new byte[] { 3, 2, 1, 0 })]
    [InlineData(2, 3, new byte[] { 3, 2, 1 })]
    [InlineData(4, 2, new byte[] { 10, 5 })]
    [InlineData(4, 3, new byte[] { 10, 5, 11 })]
    public void PackThenUnpack_RoundTrip_PreservesData(int bitDepth, int pixelCount, byte[] samples)
    {
        var packedLength = (pixelCount * bitDepth + 7) / 8;
        Span<byte> packed = new byte[packedLength];
        BitDepthConverter.PackScanline(samples, packed, bitDepth, pixelCount);

        Span<byte> unpacked = new byte[pixelCount];
        BitDepthConverter.UnpackScanline(packed, unpacked, bitDepth, pixelCount);

        Assert.Equal(samples, unpacked.ToArray());
    }
}
