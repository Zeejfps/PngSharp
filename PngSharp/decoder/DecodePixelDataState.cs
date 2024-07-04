using System.IO.Compression;

namespace PngSharp.PngSharp;

internal sealed class DecodePixelDataState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        Console.WriteLine($"Executing {GetType()} State");

        var compressedPixelData = decoder.CompressedPixelDataStream;
        compressedPixelData.Seek(2, SeekOrigin.Begin);
        using var deflateStream = new DeflateStream(compressedPixelData, CompressionMode.Decompress);
        var scanLineDecoder = new PngScanLineDecoder(decoder.IhdrChunkData, deflateStream);
        for (var i = 0; i < decoder.IhdrChunkData.Height; i++)
            scanLineDecoder.DecodeScanlineTo(decoder.PixelDataStream);

        decoder.State = decoder.DoneState;
    }
}