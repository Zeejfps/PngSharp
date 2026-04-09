using PngSharp.Api;
using PngSharp.Api.Exceptions;
using PngSharp.Spec;

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
        
        if (header.Id == HeaderIds.IEND)
        {
            reader.ReadAndValidateCrc(HeaderIds.IEND);
            decoder.State = decoder.DecodePixelDataState;
            return;
        }

        if (header.Id == HeaderIds.IDAT)
        {
            decoder.State = new ReadIdataChunkState(header, decoder);
            return;
        }

        if (header.Id == HeaderIds.SRGB)
        {
            var srgbData = reader.ReadSrgbChunkData();
            decoder.Srgb = srgbData;
            reader.ReadAndValidateCrc(HeaderIds.SRGB);
            return;
        }

        if (header.Id == HeaderIds.GAMA)
        {
            var gamaData = reader.ReadGamaChunkData();
            decoder.Gama = gamaData;
            reader.ReadAndValidateCrc(HeaderIds.GAMA);
            return;
        }

        if (header.Id == HeaderIds.PHYS)
        {
            var physChunkData = reader.ReadPhysChunkData();
            decoder.Phys = physChunkData;
            reader.ReadAndValidateCrc(HeaderIds.PHYS);
            return;
        }

        if (PngSpecUtils.IsCriticalChunk(header))
            throw new PngFormatException($"Unrecognized critical chunk: '{header.Id}'");

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadAndValidateCrc(header.Id);
    }
}