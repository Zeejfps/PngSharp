namespace PngSharp.Spec.Chunks.eXIf;

/// <summary>
/// eXIf chunk: raw EXIF metadata blob.
/// Data starts with "MM" (big-endian) or "II" (little-endian) TIFF byte order mark.
/// Minimum 4 bytes.
/// </summary>
public readonly record struct ExifChunkData
{
    public byte[] Data { get; init; }
}
