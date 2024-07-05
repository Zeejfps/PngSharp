namespace PngSharp.Decoder.States;

internal sealed class ReadIhdrChunkState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        var reader = decoder.Reader;
        reader.ReadChunkHeader(out var header);
        if (!PngSpec.IsIHDRChunkHeader(header))
            throw new Exception("Expected IHDR chunk");
        var data = reader.ReadIhdrChunkData(); 
        reader.ReadCrc();

        decoder.IhdrChunkData = data;
        decoder.State = new ReadChunkState(decoder);
    }
}