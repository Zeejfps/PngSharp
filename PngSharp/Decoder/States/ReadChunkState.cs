namespace PngSharp.Decoder.States;

internal class ReadChunkState(PngDecoder decoder) : IDecoderState
{
    public void Execute()
    {
        Console.WriteLine($"Executing {GetType()} State");

        var reader = decoder.Reader;
        reader.BeginReadChunk(out var header);
        Console.WriteLine(header);
        
        if (PngSpec.IsIENDChunkHeader(header))
        {
            reader.EndReadChunk();
            decoder.State = new DecodePixelDataState(decoder);
            return;
        }

        if (PngSpec.IsIDATChunkHeader(header))
        {
            decoder.State = new ReadIdataChunkState(header, decoder);
            return;
        }

        if (PngSpec.IsSRGBChunkHeader(header))
        {
            reader.ReadSrgbChunkData();
            reader.EndReadChunk();
            return;
        }

        if (PngSpec.IsGAMAChunkHeader(header))
        {
            reader.ReadGamaChunkData();
            reader.EndReadChunk();
            return;
        }
        
        if (PngSpec.IsPHYSChunkHeader(header))
        {
            reader.ReadPhysChunkData();
            reader.EndReadChunk();
            return;
        }

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.EndReadChunk();
    }
}