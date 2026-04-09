namespace PngSharp.Spec.Chunks.bKGD;

public readonly record struct BkgdChunkData
{
    /// <summary>
    /// Raw background color bytes. Interpretation depends on color type:
    /// Type 0 (Grayscale) or 4 (Grayscale+Alpha): 2 bytes — grey sample value (big-endian uint16)
    /// Type 2 (TrueColor) or 6 (RGBA): 6 bytes — R, G, B values (2 bytes each, big-endian)
    /// Type 3 (IndexedColor): 1 byte — palette index
    /// </summary>
    public byte[] Data { get; init; }
}
