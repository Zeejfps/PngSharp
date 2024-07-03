namespace PngSharp;

public static class PngSpec
{
    private static byte[] PNG_SIGNATURE = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    
    public static bool IsPngFile(ReadOnlySpan<byte> sig)
    {
        return sig.SequenceEqual(PNG_SIGNATURE.AsSpan());
    }
    
    public readonly struct ChunkHeader
    {
        public uint ChunkSizeInBytes { get; init; }
        public string Name { get; init; }

        public override string ToString()
        {
            return $"{nameof(ChunkSizeInBytes)}: {ChunkSizeInBytes}, {nameof(Name)}: {Name}";
        }
    }
    
    public readonly struct ImageData
    {
        public uint Width { get; init; }
        public uint Height { get; init; }

        public byte BitDepth { get; init; }
        public ColorType ColorType { get; init; }
        public CompressionMethod CompressionMethod { get; init; }
        public FilterMethod FilterMethod { get; init; }
        public InterlaceMethod InterlaceMethod { get; init; }
        

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

    public enum RenderingIntent : byte
    {
        Perceptual,
        RelativeColorimetric,
        Saturation,
        AbsoluteColorimetric
    }
    
    public enum InterlaceMethod : byte
    {
        /// <summary>
        /// No interlace.
        /// </summary>
        None = 0,
        /// <summary>
        /// Adam7 interlace.
        /// </summary>
        Adam7 = 1
    }
    
    [Flags]
    public enum ColorType : byte
    {
        /// <summary>
        /// Grayscale.
        /// </summary>
        None = 0,
        /// <summary>
        /// Colors are stored in a palette rather than directly in the data.
        /// </summary>
        PaletteUsed = 1,
        /// <summary>
        /// The image uses color.
        /// </summary>
        ColorUsed = 2,
        /// <summary>
        /// The image has an alpha channel.
        /// </summary>
        AlphaChannelUsed = 4
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
}