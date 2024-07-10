using PngSharp.Spec;

namespace PngSharp.Decoder.States;

internal sealed class ReadIhdrChunkState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    
    public ReadIhdrChunkState(PngDecoder decoder)
    {
        m_Decoder = decoder;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var reader = decoder.Reader;
        reader.ReadChunkHeader(out var header);
        if (header.Name != HeaderNames.IHDR)
            throw new Exception("Expected IHDR chunk");
        var data = reader.ReadIhdrChunkData(); 
        var crc = reader.ReadCrc();
        Console.WriteLine($"IHDR Chunk CRC: {crc}");

        decoder.DecodedPng.Width = (int)data.Width;
        decoder.DecodedPng.Height = (int)data.Height;
        decoder.DecodedPng.ColorType = data.ColorType;
        decoder.IhdrChunkData = data;
        decoder.State = new ReadChunkState(decoder);
    }
}