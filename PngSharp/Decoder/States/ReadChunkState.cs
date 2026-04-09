using PngSharp.Api;
using PngSharp.Api.Exceptions;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;

namespace PngSharp.Decoder.States;

internal sealed class ReadChunkState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    private readonly ILogger m_Logger;
    
    public ReadChunkState(PngDecoder decoder, ILogger logger)
    {
        m_Decoder = decoder;
        m_Logger = logger;
    }
    
    public void Execute()
    {
        var decoder = m_Decoder;
        var reader = decoder.Reader;
        reader.ReadChunkHeader(out var header);
        m_Logger.Debug(header.ToString());
        
        if (PngSpecUtils.IsIENDChunkHeader(header))
        {
            reader.ReadAndValidateCrc(HeaderIds.IEND);
            decoder.State = decoder.DecodePixelDataState;
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
            decoder.Srgb = AncillaryChunk<SrgbChunkData>.Of(srgbData);
            reader.ReadAndValidateCrc(HeaderIds.SRGB);
            return;
        }

        if (PngSpecUtils.IsGAMAChunkHeader(header))
        {
            var gamaData = reader.ReadGamaChunkData();
            decoder.Gama = AncillaryChunk<GammaChunkData>.Of(gamaData);
            reader.ReadAndValidateCrc(HeaderIds.GAMA);
            return;
        }
        
        if (PngSpecUtils.IsPHYSChunkHeader(header))
        {
            var physChunkData = reader.ReadPhysChunkData();
            decoder.Phys = AncillaryChunk<PhysChunkData>.Of(physChunkData);
            reader.ReadAndValidateCrc(HeaderIds.PHYS);
            return;
        }

        if (PngSpecUtils.IsCriticalChunk(header))
            throw new PngFormatException($"Unrecognized critical chunk: '{header.Id}'");

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadAndValidateCrc(header.Id);
    }
}