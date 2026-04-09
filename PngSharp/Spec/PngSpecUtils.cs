namespace PngSharp.Spec;

internal static class PngSpecUtils
{
    public static byte[] PNG_SIGNATURE = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

    public static bool IsValidPngFileSignature(ReadOnlySpan<byte> sig)
    {
        return sig.SequenceEqual(PNG_SIGNATURE.AsSpan());
    }

    public static bool IsIENDChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Id == HeaderIds.IEND;
    }

    public static bool IsIDATChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Id == HeaderIds.IDAT;
    }

    public static bool IsSRGBChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Id == HeaderIds.SRGB;
    }

    public static bool IsGAMAChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Id == HeaderIds.GAMA;
    }

    public static bool IsPHYSChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Id == HeaderIds.PHYS;
    }

    public static bool IsCriticalChunk(ChunkHeader chunkHeader)
    {
        return char.IsUpper(chunkHeader.Id[0]);
    }
}