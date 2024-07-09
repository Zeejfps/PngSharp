using PngSharp.Api;
using PngSharp.Common;
using PngSharp.Spec;

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
        
        if (PngSpecUtils.IsIENDChunkHeader(header))
        {
            reader.ReadCrc();
            decoder.State = new DecodePixelDataState(decoder);
            return;
        }

        if (PngSpecUtils.IsIDATChunkHeader(header))
        {
            decoder.State = new ReadIdataChunkState(header, decoder);
            return;
        }

        if (PngSpecUtils.IsSRGBChunkHeader(header))
        {
            var srgbData = reader.ReadSrgbChunkData();
            decoder.DecodedPng.Srgb = AncillaryChunk<SrgbChunkData>.Of(srgbData);
            reader.ReadCrc();
            return;
        }

        if (PngSpecUtils.IsGAMAChunkHeader(header))
        {
            var gamaData = reader.ReadGamaChunkData();
            decoder.DecodedPng.Gama = AncillaryChunk<GammaChunkData>.Of(gamaData);
            reader.ReadCrc();
            return;
        }
        
        if (PngSpecUtils.IsPHYSChunkHeader(header))
        {
            var physChunkData = reader.ReadPhysChunkData();
            decoder.DecodedPng.Phys = AncillaryChunk<PhysChunkData>.Of(physChunkData);
            reader.ReadCrc();
            return;
        }

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadCrc();
    }
}