namespace PngSharp.Decoder.States;

internal sealed class ReadIdataChunkState(PngSpec.ChunkHeader header, PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        var reader = decoder.Reader;
        reader.ReadIdatChunkDataIntoStream(header, decoder.CompressedPixelDataStream);
        reader.ReadCrc();
        decoder.State = decoder.ReadChunkState;
    }
}