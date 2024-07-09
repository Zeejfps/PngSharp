namespace PngSharp.Spec;

public static class PngSpecUtils
{
    public static class HeaderNames
    {
        public const string IHDR = "IHDR";
        public const string IEND = "IEND";
        public const string IDAT = "IDAT";
        public const string PLTE = "PLTE";
        public const string SRGB = "sRGB";
        public const string GAMA = "gAMA";
        public const string PHYS = "pHYs";
    }
   
    public static byte[] PNG_SIGNATURE = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};

    public static bool IsValidPngFileSignature(ReadOnlySpan<byte> sig)
    {
        return sig.SequenceEqual(PNG_SIGNATURE.AsSpan());
    }

    public static bool IsIHDRChunkHeader(ChunkHeader header)
    {
        return header.Name == HeaderNames.IHDR;
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
    
    public static bool IsPLTEChunkHeader(ChunkHeader chunkHeader)
    {
        return chunkHeader.Name == HeaderNames.PLTE;
    }
}