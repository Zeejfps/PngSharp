namespace PngSharp.PngSharp;

interface IDecoderState
{
    void Execute();
}

internal class ReadSignatureState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        var sig = decoder.Reader.ReadSignature();
        if (!PngSpec.IsValidPngFileSignature(sig))
            throw new Exception("Not a png file");
        decoder.State = decoder.ReadIhdrChunkState;
    } 
}