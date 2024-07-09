using System.IO.Compression;
using PngSharp.Common.AdaptiveFilter;

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
        decoder.State = decoder.DoneState;
    }
}