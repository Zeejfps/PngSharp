using System.IO.Compression;

namespace PngSharp.Spec.Chunks.iCCP;

/// <summary>
/// iCCP chunk: embedded ICC color profile.
/// ProfileName is 1-79 Latin-1 characters.
/// CompressedProfile is the deflate-compressed ICC profile bytes.
/// </summary>
public readonly record struct IccpChunkData
{
    public string ProfileName { get; init; }
    public byte[] CompressedProfile { get; init; }

    public byte[] DecompressProfile()
    {
        using var compressedStream = new MemoryStream(CompressedProfile);
        using var deflateStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        deflateStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    public static IccpChunkData Create(string profileName, byte[] rawProfile)
    {
        using var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionLevel.Optimal, true))
        {
            zlibStream.Write(rawProfile);
        }
        return new IccpChunkData
        {
            ProfileName = profileName,
            CompressedProfile = compressedStream.ToArray(),
        };
    }
}
