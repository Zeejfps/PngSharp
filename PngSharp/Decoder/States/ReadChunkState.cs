namespace PngSharp.Decoder.States;

internal sealed class ReadChunkState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    
    public ReadChunkState(PngDecoder decoder)
    {
        m_Decoder = decoder;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var reader = decoder.Reader;
        reader.ReadChunkHeader(out var header);
        Console.WriteLine(header);
        
        if (PngSpec.IsIENDChunkHeader(header))
        {
            reader.ReadCrc();
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
            reader.ReadCrc();
            return;
        }

        if (PngSpec.IsGAMAChunkHeader(header))
        {
            reader.ReadGamaChunkData();
            reader.ReadCrc();
            return;
        }
        
        if (PngSpec.IsPHYSChunkHeader(header))
        {
            reader.ReadPhysChunkData();
            reader.ReadCrc();
            return;
        }

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadCrc();
    }
}