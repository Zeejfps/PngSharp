namespace PngSharp.Spec;

internal static class BitDepthConverter
{
    /// <summary>
    /// Unpacks a scanline from packed bytes to 1-byte-per-sample.
    /// PNG spec: leftmost sample in the high-order bits of each byte (MSB first).
    /// </summary>
    public static void UnpackScanline(ReadOnlySpan<byte> packed, Span<byte> output, int bitDepth, int pixelCount)
    {
        var mask = (1 << bitDepth) - 1;
        var pixelsPerByte = 8 / bitDepth;

        for (var i = 0; i < pixelCount; i++)
        {
            var byteIndex = i / pixelsPerByte;
            var bitOffset = (pixelsPerByte - 1 - (i % pixelsPerByte)) * bitDepth;
            output[i] = (byte)((packed[byteIndex] >> bitOffset) & mask);
        }
    }

    /// <summary>
    /// Packs a scanline from 1-byte-per-sample to packed bytes.
    /// PNG spec: leftmost sample in the high-order bits of each byte (MSB first).
    /// Unused low-order bits in the last byte are zero-filled.
    /// </summary>
    public static void PackScanline(ReadOnlySpan<byte> unpacked, Span<byte> output, int bitDepth, int pixelCount)
    {
        output.Clear();
        var pixelsPerByte = 8 / bitDepth;

        for (var i = 0; i < pixelCount; i++)
        {
            var byteIndex = i / pixelsPerByte;
            var bitOffset = (pixelsPerByte - 1 - (i % pixelsPerByte)) * bitDepth;
            output[byteIndex] |= (byte)(unpacked[i] << bitOffset);
        }
    }
}
