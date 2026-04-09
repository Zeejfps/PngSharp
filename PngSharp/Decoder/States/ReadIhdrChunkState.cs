using PngSharp.Api;
using PngSharp.Api.Exceptions;
using PngSharp.Spec;

namespace PngSharp.Decoder.States;

internal sealed class ReadIhdrChunkState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    private readonly ILogger m_Logger;
    
    public ReadIhdrChunkState(PngDecoder decoder, ILogger logger)
    {
        m_Decoder = decoder;
        m_Logger = logger;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var reader = decoder.Reader;
        reader.ReadChunkHeader(out var header);
        if (header.Id != HeaderIds.IHDR)
            throw new PngFormatException("Expected IHDR chunk");
        var data = reader.ReadIhdrChunkData();
        reader.ReadAndValidateCrc(HeaderIds.IHDR);

        decoder.IhdrChunkData = data;
        decoder.State = decoder.ReadChunkState;
    }
}