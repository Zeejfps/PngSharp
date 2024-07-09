using System.IO.Compression;
using PngSharp.AdaptiveFilter;

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
        var compressedPixelData = decoder.CompressedPixelDataStream;
        compressedPixelData.Seek(0, SeekOrigin.Begin);
        using var decompressionStream = new ZLibStream(compressedPixelData, CompressionMode.Decompress);

        var adaptiveFilter = new PngAdaptiveFilter(
            (int)decoder.IhdrChunkData.Width,
            (int)decoder.IhdrChunkData.Height,
            decoder.IhdrChunkData.GetBytesPerPixel());
        
        adaptiveFilter.Reverse(decoder.PixelDataStream, decompressionStream);
        //decoder.PixelDataStream.Flush();
        
        // var scanLineDecoder = new PngScanLineDecoder(decoder.IhdrChunkData, deflateStream);
        // for (var i = 0; i < decoder.IhdrChunkData.Height; i++)
        //     scanLineDecoder.DecodeScanlineTo(decoder.PixelDataStream);

        decoder.State = decoder.DoneState;
    }
}