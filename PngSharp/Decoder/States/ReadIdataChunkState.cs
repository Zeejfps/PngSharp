namespace PngSharp.Decoder.States;

internal sealed class ReadIdataChunkState : IDecoderState
{
    private readonly PngSpec.ChunkHeader m_Header;
    private readonly PngDecoder m_Decoder;
    
    public ReadIdataChunkState(PngSpec.ChunkHeader header, PngDecoder decoder)
    {
        m_Header = header;
        m_Decoder = decoder;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var header = m_Header;
        var reader = decoder.Reader;
        reader.ReadIdatChunkDataIntoStream(header, decoder.CompressedPixelDataStream);
        reader.ReadCrc();
        decoder.State = decoder.ReadChunkState;
    }
}