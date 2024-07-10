namespace PngSharp.Spec;

public static class PngSpecUtils
{
    public static byte[] PNG_SIGNATURE = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

    public static bool IsValidPngFileSignature(ReadOnlySpan<byte> sig)
    {
        return sig.SequenceEqual(PNG_SIGNATURE.AsSpan());
    }

    public static bool IsIENDChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.IEND;
    }

    public static bool IsIDATChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.IDAT;
    }

    public static bool IsSRGBChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.SRGB;
    }

    public static bool IsGAMAChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.GAMA;
    }

    public static bool IsPHYSChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.PHYS;
    }
}