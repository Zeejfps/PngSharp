namespace PngSharp.Spec.Chunks.PLTE;

public readonly record struct PlteChunkData
{
    /// <summary>
    /// Flat RGB byte array: [R0,G0,B0, R1,G1,B1, ...]
    /// Length must be divisible by 3.
    /// </summary>
    public byte[] Entries { get; init; }

    public int EntryCount => Entries.Length / 3;
}
