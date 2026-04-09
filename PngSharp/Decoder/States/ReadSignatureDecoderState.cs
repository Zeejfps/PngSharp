using PngSharp.Api.Exceptions;
using PngSharp.Spec;

namespace PngSharp.Decoder.States;

internal sealed class ReadSignatureState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    
    public ReadSignatureState(PngDecoder decoder)
    {
        m_Decoder = decoder;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var sig = decoder.Reader.ReadSignature();
        if (!PngSpecUtils.IsValidPngFileSignature(sig))
            throw new PngFormatException("Not a png file");
        decoder.State = decoder.ReadIhdrChunkState;
    } 
}