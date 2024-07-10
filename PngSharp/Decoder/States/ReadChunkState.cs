using PngSharp.Api;
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
            var crc = reader.CurrentCrcValue;
            var newCrc = reader.ReadCrc();
            m_Logger.Debug($"Our CRC: {crc}, Read CRC: {newCrc}");
            return;
        }

        if (PngSpecUtils.IsGAMAChunkHeader(header))
        {
            var gamaData = reader.ReadGamaChunkData();
            decoder.DecodedPng.Gama = AncillaryChunk<GammaChunkData>.Of(gamaData);
            var crc = reader.CurrentCrcValue;
            var newCrc = reader.ReadCrc();
            m_Logger.Debug($"Our CRC: {crc}, Read CRC: {newCrc}");
            return;
        }
        
        if (PngSpecUtils.IsPHYSChunkHeader(header))
        {
            var physChunkData = reader.ReadPhysChunkData();
            decoder.DecodedPng.Phys = AncillaryChunk<PhysChunkData>.Of(physChunkData);
            var crc = reader.CurrentCrcValue;
            var newCrc = reader.ReadCrc();
            m_Logger.Debug($"Our CRC: {crc}, Read CRC: {newCrc}");
            return;
        }

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadCrc();
    }
}