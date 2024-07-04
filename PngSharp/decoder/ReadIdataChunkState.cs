namespace PngSharp.PngSharp;

internal sealed class ReadIdataChunkState(PngSpec.ChunkHeader header, PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        var reader = decoder.Reader;
        reader.ReadIdatChunkDataIntoStream(header, decoder.PixelDataStream);
        reader.EndReadChunk();
        decoder.State = decoder.ReadChunkState;
    }
}