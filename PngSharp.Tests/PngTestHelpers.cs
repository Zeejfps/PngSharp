using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Tests;

public static class PngTestHelpers
{
    public static IRawPng RoundTrip(IRawPng png)
    {
        return Png.DecodeFromByteArray(Png.EncodeToByteArray(png));
    }

    public static IhdrChunkData CreateIhdr(ColorType colorType, byte bitDepth = 8)
    {
        return new IhdrChunkData
        {
            Width = 2,
            Height = 2,
            BitDepth = bitDepth,
            ColorType = colorType,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            InterlaceMethod = InterlaceMethod.None,
        };
    }

    public static int CountChunks(byte[] pngData, string chunkType)
    {
        var count = 0;
        var i = 8; // skip PNG signature
        while (i + 8 <= pngData.Length)
        {
            var length = (pngData[i] << 24) | (pngData[i + 1] << 16) | (pngData[i + 2] << 8) | pngData[i + 3];
            var type = System.Text.Encoding.ASCII.GetString(pngData, i + 4, 4);
            if (type == chunkType)
                count++;
            if (type == "IEND")
                break;
            i += 4 + 4 + length + 4; // length field + type + data + crc
        }
        return count;
    }
}
