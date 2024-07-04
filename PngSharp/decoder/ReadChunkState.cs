namespace PngSharp.PngSharp;

internal class ReadChunkState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        var reader = decoder.Reader;
        reader.BeginReadChunk(out var header);
        if (PngSpec.IsIENDChunkHeader(header))
        {
            decoder.State = new DecompressAndUnfilterDataState(decoder);
            return;
        }

        if (PngSpec.IsIDATChunkHeader(header))
        {
            decoder.State = new ReadIdataChunkState(header, decoder);
            return;
        }
    }
}