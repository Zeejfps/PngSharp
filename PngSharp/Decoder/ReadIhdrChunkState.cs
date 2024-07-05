namespace PngSharp.Decoder;

internal class ReadIhdrChunkState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        Console.WriteLine($"Executing {GetType()} State");

        var reader = decoder.Reader;
        reader.BeginReadChunk(out var header);
        if (!PngSpec.IsIHDRChunkHeader(header))
            throw new Exception("Expected IHDR chunk");
        var data = reader.ReadIhdrChunkData();
        reader.EndReadChunk();

        decoder.IhdrChunkData = data;
        decoder.State = new ReadChunkState(decoder);
    }
}