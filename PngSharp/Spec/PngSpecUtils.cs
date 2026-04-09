namespace PngSharp.Spec;

internal static class PngSpecUtils
{
    public static byte[] PNG_SIGNATURE = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

    public static bool IsValidPngFileSignature(ReadOnlySpan<byte> sig)
    {
        return sig.SequenceEqual(PNG_SIGNATURE.AsSpan());
    }

    public static bool IsCriticalChunk(ChunkHeader chunkHeader)
    {
        return char.IsUpper(chunkHeader.Id[0]);
    }
}