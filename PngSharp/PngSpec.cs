namespace PngSharp;

public static class PngSpec
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
   
    private static byte[] PNG_SIGNATURE = {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};
    
    public readonly struct ChunkHeader
    {
        public int ChunkSizeInBytes { get; init; }
        public string Name { get; init; }

        public override string ToString()
        {
            return $"{nameof(ChunkSizeInBytes)}: {ChunkSizeInBytes}, {nameof(Name)}: {Name}";
        }
    }
    
    public readonly struct IhdrChunkData
    {
        public uint Width { get; init; }
        public uint Height { get; init; }

        public byte BitDepth { get; init; }
        public ColorType ColorType { get; init; }
        public CompressionMethod CompressionMethod { get; init; }
        public FilterMethod FilterMethod { get; init; }
        public InterlaceMethod InterlaceMethod { get; init; }


        public int GetBytesPerPixel()
        {
            return ColorType switch
            {
                ColorType.Grayscale => 1,
                ColorType.TrueColor => 3,
                ColorType.IndexedColor => 1,
                ColorType.GrayscaleWithAlpha => 2,
                ColorType.TrueColorWithAlpha => 4,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public override string ToString()
        {
            return $"{nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(BitDepth)}: {BitDepth}, {nameof(ColorType)}: {ColorType}, {nameof(CompressionMethod)}: {CompressionMethod}, {nameof(FilterMethod)}: {FilterMethod}, {nameof(InterlaceMethod)}: {InterlaceMethod}";
        }
    }
    
    public readonly struct SrgbChunkData
    {
        public RenderingIntent RenderingIntent { get; init; }

        public override string ToString()
        {
            return $"{nameof(RenderingIntent)}: {RenderingIntent}";
        }
    }
    
    public readonly struct GammaChunkData
    {
        public uint Value { get; init; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

    public readonly struct PhysChunkData
    {
        
    }

    public enum RenderingIntent : byte
    {
        Perceptual,
        RelativeColorimetric,
        Saturation,
        AbsoluteColorimetric
    }
    
    public enum InterlaceMethod : byte
    {
        None = 0,
        Adam7 = 1
    }
    
    public enum ColorType : byte
    {
        Grayscale = 0,
        TrueColor = 2,
        IndexedColor = 3,
        GrayscaleWithAlpha = 4,
        TrueColorWithAlpha = 6,
    }
    
    public enum CompressionMethod : byte
    {
        DeflateWithSlidingWindow = 0
    }
    
    public enum FilterMethod : byte
    {
        AdaptiveFiltering = 0
    }

    public enum AdaptiveFilteringType : byte
    {
        None,
        Sub,
        Up,
        Average,
        Paeth
    }
    
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