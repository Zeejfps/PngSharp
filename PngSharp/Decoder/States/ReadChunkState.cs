using PngSharp.Api;
using PngSharp.Common;

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
            var srgbData = reader.ReadSrgbChunkData();
            decoder.DecodedPng.Srgb = AncillaryChunk<PngSpec.SrgbChunkData>.Of(srgbData);
            reader.ReadCrc();
            return;
        }

        if (PngSpec.IsGAMAChunkHeader(header))
        {
            var gamaData = reader.ReadGamaChunkData();
            decoder.DecodedPng.Gama = AncillaryChunk<PngSpec.GammaChunkData>.Of(gamaData);
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