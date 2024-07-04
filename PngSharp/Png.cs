using System.IO.Compression;

namespace PngSharp;

public interface IPngImageDataContainer
{
    void Load(int width, int height, byte[] pixelData);
}

public static class Png
{
    public static void LoadFromFile(this IPngImageDataContainer container, string pathToFile)
    {
        var reader = PngReader.ReadFromFile(pathToFile);
        var sig = reader.ReadSignature();

        var isPng = PngSpec.IsValidPngFileSignature(sig);
        if (!isPng)
            throw new Exception("Invalid PNG signature");
        
        reader.BeginReadChunk(out var chunkHeader);
        if (!PngSpec.IsIHDRChunkHeader(chunkHeader))
            throw new Exception($"Malformed png file. Expected IHDR chunk, Got: {chunkHeader.Name}");
        var ihgrChunkData = reader.ReadIhdrChunkData();
        reader.EndReadChunk();
        
        reader.BeginReadChunk(out chunkHeader);
        Console.WriteLine(chunkHeader);
        var sRgbChunkData = reader.ReadSrgbChunkData();
        reader.EndReadChunk();
        
        reader.BeginReadChunk(out chunkHeader);
        Console.WriteLine(chunkHeader);
        var gamaChunkData = reader.ReadGamaChunkData();
        reader.EndReadChunk();

        reader.BeginReadChunk(out chunkHeader);
        Console.WriteLine(chunkHeader);
        var physChunkData = reader.ReadPhysChunkData();
        reader.EndReadChunk();

        using var imageDataStream = new MemoryStream();
        reader.BeginReadChunk(out chunkHeader);
        while (PngSpec.IsIDATChunkHeader(chunkHeader))
        {
            reader.ReadIdatChunkDataIntoStream(chunkHeader, imageDataStream);
            reader.EndReadChunk();
            reader.BeginReadChunk(out chunkHeader);
            Console.WriteLine(chunkHeader);
        }
        reader.EndReadChunk();

        // NOTE(Zee): 2 offset here is because of the ZLIB header
        imageDataStream.Seek(2, SeekOrigin.Begin);
        using var deflateStream = new DeflateStream(imageDataStream, CompressionMode.Decompress);
        var decoder = new PngScanLineDecoder(ihgrChunkData, deflateStream);

        var outputStream = new MemoryStream();
        for (var i = 0; i < ihgrChunkData.Height; i++)
            decoder.DecodeScanlineTo(outputStream);
        
        var pixelData = outputStream.ToArray();
        container.Load((int)ihgrChunkData.Width, (int)ihgrChunkData.Height, pixelData);
    }
}