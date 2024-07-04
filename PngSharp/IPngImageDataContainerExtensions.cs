using System.IO.Compression;

namespace PngSharp;

public interface IPngImageDataContainer
{
    void Load(int width, int height, byte[] pixelData);
}

public static class IPngImageDataContainerExtensions
{
    public static void LoadFromFile(this IPngImageDataContainer container, string pathToFile)
    {
        var reader = PngReader.ReadFromFile(pathToFile);
        var sig = reader.ReadSig();

        var isPng = PngSpec.IsPngFile(sig);
        if (!isPng)
            throw new Exception("Invalid PNG signature");
        
        PngSpec.ImageData imageData;
        reader.BeginReadChunk(out var header);
        {
            Console.WriteLine(header);
            imageData = reader.ReadIhdrChunkData();
            Console.WriteLine($"{header.Name} Data: {imageData}");
        }
        reader.EndReadChunk();

        reader.BeginReadChunk(out header);
        {
            var data = reader.ReadSrgbChunkData();
        }
        reader.EndReadChunk();

        reader.BeginReadChunk(out header);
        {
            var data = reader.ReadGamaChunkData();
        }
        reader.EndReadChunk();

        reader.BeginReadChunk(out header);
        {
            var data = reader.ReadPhysChunkData();
        }
        reader.EndReadChunk();

        using var imageDataStream = new MemoryStream();
        reader.BeginReadChunk(out header);
        {
            reader.ReadIdatChunkDataIntoStream(header, imageDataStream);
        }
        reader.EndReadChunk();

        reader.BeginReadChunk(out header);
        {
            Console.WriteLine(header);
        }
        reader.EndReadChunk();

        // NOTE(Zee): 2 offset here is because of the ZLIB header
        imageDataStream.Seek(2, SeekOrigin.Begin);
        using var decompressedDataStream = new MemoryStream();
        using (var deflateStream = new DeflateStream(imageDataStream, CompressionMode.Decompress))
        {
            deflateStream.CopyTo(decompressedDataStream);
        }

        decompressedDataStream.Position = 0;
        var decoder = new PngScanLineDecoder(imageData, decompressedDataStream);

        var outputStream = new MemoryStream();
        for (var i = 0; i < imageData.Height; i++)
            decoder.DecodeScanlineTo(outputStream);
        
        var pixelData = outputStream.ToArray();
        container.Load((int)imageData.Width, (int)imageData.Height, pixelData);
    }
}