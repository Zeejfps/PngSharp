namespace PngSharp.Decoder.States;

internal class ReadSignatureState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        Console.WriteLine($"Executing {GetType()} State");

        var sig = decoder.Reader.ReadSignature();
        if (!PngSpec.IsValidPngFileSignature(sig))
            throw new Exception("Not a png file");
        decoder.State = decoder.ReadIhdrChunkState;
    } 
}