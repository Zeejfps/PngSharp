namespace PngSharp.Spec.Chunks.sBIT;

/// <summary>
/// sBIT chunk: records the original number of significant bits per channel.
/// Data length depends on color type:
/// Type 0 (Grayscale): 1 byte
/// Type 2 (TrueColor): 3 bytes (R, G, B)
/// Type 3 (IndexedColor): 3 bytes (R, G, B)
/// Type 4 (GrayscaleWithAlpha): 2 bytes (grey, alpha)
/// Type 6 (TrueColorWithAlpha): 4 bytes (R, G, B, A)
/// Each value must be > 0 and &lt;= bit depth (or 8 for indexed).
/// </summary>
public readonly record struct SbitChunkData
{
    public byte[] Data { get; init; }
}
