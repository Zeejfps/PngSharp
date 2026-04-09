namespace PngSharp.Api.Exceptions;

public class PngCorruptException : PngFormatException
{
    public string ChunkId { get; }
    public uint ComputedCrc { get; }
    public uint ExpectedCrc { get; }

    public PngCorruptException(string chunkId, uint computedCrc, uint expectedCrc)
        : base($"CRC mismatch for chunk '{chunkId}': computed 0x{computedCrc:X8}, expected 0x{expectedCrc:X8}")
    {
        ChunkId = chunkId;
        ComputedCrc = computedCrc;
        ExpectedCrc = expectedCrc;
    }
}
