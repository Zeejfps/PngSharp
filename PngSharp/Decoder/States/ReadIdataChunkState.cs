namespace PngSharp.Decoder.States;

internal sealed class ReadIdataChunkState(PngSpec.ChunkHeader header, PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        Console.WriteLine($"Executing {GetType()} State");
        var reader = decoder.Reader;
        reader.ReadIdatChunkDataIntoStream(header, decoder.CompressedPixelDataStream);
        reader.EndReadChunk();
        decoder.State = decoder.ReadChunkState;
    }
}