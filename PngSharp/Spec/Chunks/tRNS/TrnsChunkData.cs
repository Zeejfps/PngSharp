namespace PngSharp.Spec.Chunks.tRNS;

public readonly record struct TrnsChunkData
{
    /// <summary>
    /// Raw transparency bytes. Interpretation depends on color type:
    /// Type 0 (Grayscale): 2 bytes — transparent grey sample value
    /// Type 2 (TrueColor): 6 bytes — transparent R, G, B values (2 bytes each, big-endian)
    /// Type 3 (IndexedColor): 1 byte per palette entry — alpha value (0=transparent, 255=opaque)
    /// </summary>
    public byte[] Data { get; init; }
}
