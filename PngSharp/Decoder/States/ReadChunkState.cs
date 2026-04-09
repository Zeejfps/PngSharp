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

        if (header.Id == HeaderIds.PLTE)
        {
            var plteData = reader.ReadPlteChunkData(header.ChunkSizeInBytes);
            decoder.Plte = plteData;
            reader.ReadAndValidateCrc(HeaderIds.PLTE);
            return;
        }

        if (header.Id == HeaderIds.TRNS)
        {
            var trnsData = reader.ReadTrnsChunkData(header.ChunkSizeInBytes);
            decoder.Trns = trnsData;
            reader.ReadAndValidateCrc(HeaderIds.TRNS);
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

        if (header.Id == HeaderIds.TEXT)
        {
            var textData = reader.ReadTxtChunkData(header.ChunkSizeInBytes);
            decoder.TxtChunks.Add(textData);
            reader.ReadAndValidateCrc(HeaderIds.TEXT);
            return;
        }

        if (header.Id == HeaderIds.ZTXT)
        {
            var textData = reader.ReadZtxtChunkData(header.ChunkSizeInBytes);
            decoder.ZTxtChunks.Add(textData);
            reader.ReadAndValidateCrc(HeaderIds.ZTXT);
            return;
        }

        if (header.Id == HeaderIds.ITXT)
        {
            var textData = reader.ReadItxtChunkData(header.ChunkSizeInBytes);
            decoder.ITxtChunks.Add(textData);
            reader.ReadAndValidateCrc(HeaderIds.ITXT);
            return;
        }

        if (PngSpecUtils.IsCriticalChunk(header))
            throw new PngFormatException($"Unrecognized critical chunk: '{header.Id}'");

        reader.ReadChunkData(header.ChunkSizeInBytes);
        reader.ReadAndValidateCrc(header.Id);
    }
}