// See https://aka.ms/new-console-template for more information

using System.IO.Compression;
using System.Text;
using PngSharp;

var reader = PngReader.ReadFromFile("Assets/sprite_atlas_128x64.png");
var sig = reader.ReadSig();

Console.WriteLine($"Sig: {ToHexString(sig)}");


var isPng = PngSpec.IsPngFile(sig);
if (!isPng)
{
    Console.WriteLine("Not a PNG file");
    return;
}

Console.WriteLine("Found a PNG file");

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
    Console.WriteLine(header);
    var data = reader.ReadSrgbChunkData();
    Console.WriteLine($"{header.Name} Data: {data}");
}
reader.EndReadChunk();

reader.BeginReadChunk(out header);
{
    Console.WriteLine(header);
    var data = reader.ReadGamaChunkData();
    Console.WriteLine($"{header.Name} Data: {data}");
}
reader.EndReadChunk();

reader.BeginReadChunk(out header);
{
    Console.WriteLine(header);
    var data = reader.ReadPhysChunkData();
    Console.WriteLine($"{header.Name} Data: {data}");
}
reader.EndReadChunk();

reader.BeginReadChunk(out header);
{
    Console.WriteLine(header);
    using var memoryStream = new MemoryStream();
    reader.ReadIdatChunkDataIntoStream(header, memoryStream);
    Console.WriteLine($"Compressed data SizeInBytes: {memoryStream.Length}");
    // NOTE(Zee): 2 offset here is because of the ZLIB header
    memoryStream.Seek(2, SeekOrigin.Begin);
    using var decompressedDataStream = new MemoryStream();
    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
    {
        deflateStream.CopyTo(decompressedDataStream);
        Console.WriteLine("Decompressed data SizeInBytes: " + decompressedDataStream.Length);
    }

    decompressedDataStream.Position = 0;
    var decoder = new PngDecoder(imageData, decompressedDataStream);

    var outputStream = new MemoryStream();
    for (var i = 0; i < imageData.Height; i++)
    {
        decoder.DecodeScanlineTo(outputStream);
    }
    
    Console.WriteLine(outputStream.Length);

    var pixelData = outputStream.ToArray();
    Console.WriteLine(ToHexString(pixelData.AsSpan(4 * 128*4 + (4*2), 4)));

    SaveToPAM("test.pam", pixelData, (int)imageData.Width, (int)imageData.Height);
}
reader.EndReadChunk();

reader.BeginReadChunk(out header);
{
    Console.WriteLine(header);
}
reader.EndReadChunk();


string ToHexString(ReadOnlySpan<byte> bytes)
{
    return string.Concat(bytes.ToArray().Select(b => b.ToString("X2")));
}


void SaveToPAM(string filename, byte[] rgbaData, int width, int height)
{
    using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
    using (BinaryWriter bw = new BinaryWriter(fs))
    {
        // Write PAM header
        bw.Write(Encoding.ASCII.GetBytes("P7\n"));
        bw.Write(Encoding.ASCII.GetBytes($"WIDTH {width}\n"));
        bw.Write(Encoding.ASCII.GetBytes($"HEIGHT {height}\n"));
        bw.Write(Encoding.ASCII.GetBytes("DEPTH 4\n"));
        bw.Write(Encoding.ASCII.GetBytes("MAXVAL 255\n"));
        bw.Write(Encoding.ASCII.GetBytes("TUPLTYPE RGB_ALPHA\n"));
        bw.Write(Encoding.ASCII.GetBytes("ENDHDR\n"));

        // Write image data
        bw.Write(rgbaData);
    }
}
