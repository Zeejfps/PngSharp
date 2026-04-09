using System.IO.Compression;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;

namespace PngSharp.Decoder.States;

internal sealed class DecodePixelDataState : IDecoderState
{
    private readonly PngDecoder m_Decoder;

    public DecodePixelDataState(PngDecoder decoder)
    {
        m_Decoder = decoder;
    }

    public void Execute()
    {
        var decoder = m_Decoder;
        var ihdr = decoder.IhdrChunkData;
        var compressedPixelData = decoder.CompressedPixelDataStream;
        compressedPixelData.Seek(0, SeekOrigin.Begin);
        using var decompressionStream = new ZLibStream(compressedPixelData, CompressionMode.Decompress);

        var scanlineByteWidth = ihdr.GetScanlineByteWidth();
        var bytesPerPixel = ihdr.GetBytesPerPixel();
        var adaptiveFilter = new PngAdaptiveFilter(
            (int)ihdr.Height,
            scanlineByteWidth,
            bytesPerPixel
        );

        if (ihdr.BitDepth < 8)
        {
            using var packedStream = new MemoryStream();
            adaptiveFilter.Reverse(packedStream, decompressionStream);
            packedStream.Position = 0;

            var widthInPixels = (int)ihdr.Width;
            var heightInPixels = (int)ihdr.Height;
            const int stackAllocThreshold = 1024;
            var packedRow = scanlineByteWidth <= stackAllocThreshold
                ? stackalloc byte[scanlineByteWidth]
                : new byte[scanlineByteWidth];
            var unpackedRow = widthInPixels <= stackAllocThreshold
                ? stackalloc byte[widthInPixels]
                : new byte[widthInPixels];

            for (var y = 0; y < heightInPixels; y++)
            {
                packedStream.ReadExactly(packedRow);
                BitDepthConverter.UnpackScanline(packedRow, unpackedRow, ihdr.BitDepth, widthInPixels);
                decoder.PixelDataStream.Write(unpackedRow);
            }
        }
        else
        {
            adaptiveFilter.Reverse(decoder.PixelDataStream, decompressionStream);
        }

        decoder.State = decoder.DoneState;
    }
}
