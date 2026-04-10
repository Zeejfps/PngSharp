using PngSharp.Api;
using PngSharp.Api.Exceptions;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Decoder.States;

internal sealed class ReadChunkState : IDecoderState
{
    private readonly PngDecoder m_Decoder;
    private readonly ILogger m_Logger;

    private bool m_SeenPlte;
    private bool m_SeenIdat;
    private bool m_IdatFinished;

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

        // Track IDAT consecutive requirement: if we've seen IDAT and this is not IDAT or IEND,
        // then the IDAT run is finished.
        if (m_SeenIdat && header.Id != HeaderIds.IDAT && header.Id != HeaderIds.IEND)
            m_IdatFinished = true;

        if (header.Id == HeaderIds.IEND)
        {
            reader.ReadAndValidateCrc(HeaderIds.IEND);
            decoder.State = decoder.IhdrChunkData.InterlaceMethod == InterlaceMethod.Adam7
                ? decoder.DecodeAdam7State
                : decoder.DecodePixelDataState;
            return;
        }

        if (header.Id == HeaderIds.IDAT)
        {
            if (m_IdatFinished)
                throw new PngFormatException("IDAT chunks must be consecutive.");
            m_SeenIdat = true;
            decoder.State = new ReadIdataChunkState(header, decoder);
            return;
        }

        if (header.Id == HeaderIds.PLTE)
        {
            if (m_SeenPlte)
                throw new PngFormatException("Multiple PLTE chunks are not allowed.");
            if (m_SeenIdat)
                throw new PngFormatException("PLTE chunk must appear before IDAT.");
            m_SeenPlte = true;
            var plteData = reader.ReadPlteChunkData(header.ChunkSizeInBytes);
            decoder.Plte = plteData;
            reader.ReadAndValidateCrc(HeaderIds.PLTE);
            return;
        }

        if (header.Id == HeaderIds.TRNS)
        {
            if (!m_SeenPlte && decoder.IhdrChunkData.ColorType == ColorType.IndexedColor)
                throw new PngFormatException("tRNS chunk must appear after PLTE for IndexedColor.");
            if (m_SeenIdat)
                throw new PngFormatException("tRNS chunk must appear before IDAT.");
            var trnsData = reader.ReadTrnsChunkData(header.ChunkSizeInBytes);
            decoder.Trns = trnsData;
            reader.ReadAndValidateCrc(HeaderIds.TRNS);
            return;
        }

        if (header.Id == HeaderIds.ICCP)
        {
            if (decoder.Iccp.HasValue)
                throw new PngFormatException("Multiple iCCP chunks are not allowed.");
            if (m_SeenPlte)
                throw new PngFormatException("iCCP chunk must appear before PLTE.");
            if (m_SeenIdat)
                throw new PngFormatException("iCCP chunk must appear before IDAT.");
            var iccpData = reader.ReadIccpChunkData(header.ChunkSizeInBytes);
            decoder.Iccp = iccpData;
            reader.ReadAndValidateCrc(HeaderIds.ICCP);
            return;
        }

        if (header.Id == HeaderIds.SRGB)
        {
            if (decoder.Srgb.HasValue)
                throw new PngFormatException("Multiple sRGB chunks are not allowed.");
            if (m_SeenPlte)
                throw new PngFormatException("sRGB chunk must appear before PLTE.");
            if (m_SeenIdat)
                throw new PngFormatException("sRGB chunk must appear before IDAT.");
            var srgbData = reader.ReadSrgbChunkData();
            decoder.Srgb = srgbData;
            reader.ReadAndValidateCrc(HeaderIds.SRGB);
            return;
        }

        if (header.Id == HeaderIds.GAMA)
        {
            if (decoder.Gama.HasValue)
                throw new PngFormatException("Multiple gAMA chunks are not allowed.");
            if (m_SeenPlte)
                throw new PngFormatException("gAMA chunk must appear before PLTE.");
            if (m_SeenIdat)
                throw new PngFormatException("gAMA chunk must appear before IDAT.");
            var gamaData = reader.ReadGamaChunkData();
            decoder.Gama = gamaData;
            reader.ReadAndValidateCrc(HeaderIds.GAMA);
            return;
        }

        if (header.Id == HeaderIds.PHYS)
        {
            if (decoder.Phys.HasValue)
                throw new PngFormatException("Multiple pHYs chunks are not allowed.");
            if (m_SeenIdat)
                throw new PngFormatException("pHYs chunk must appear before IDAT.");
            var physChunkData = reader.ReadPhysChunkData();
            decoder.Phys = physChunkData;
            reader.ReadAndValidateCrc(HeaderIds.PHYS);
            return;
        }

        if (header.Id == HeaderIds.CHRM)
        {
            if (decoder.Chrm.HasValue)
                throw new PngFormatException("Multiple cHRM chunks are not allowed.");
            if (m_SeenPlte)
                throw new PngFormatException("cHRM chunk must appear before PLTE.");
            if (m_SeenIdat)
                throw new PngFormatException("cHRM chunk must appear before IDAT.");
            var chrmData = reader.ReadChrmChunkData();
            decoder.Chrm = chrmData;
            reader.ReadAndValidateCrc(HeaderIds.CHRM);
            return;
        }

        if (header.Id == HeaderIds.SBIT)
        {
            if (decoder.Sbit.HasValue)
                throw new PngFormatException("Multiple sBIT chunks are not allowed.");
            if (m_SeenPlte)
                throw new PngFormatException("sBIT chunk must appear before PLTE.");
            if (m_SeenIdat)
                throw new PngFormatException("sBIT chunk must appear before IDAT.");
            var sbitData = reader.ReadSbitChunkData(header.ChunkSizeInBytes);
            decoder.Sbit = sbitData;
            reader.ReadAndValidateCrc(HeaderIds.SBIT);
            return;
        }

        if (header.Id == HeaderIds.TIME)
        {
            if (decoder.Time.HasValue)
                throw new PngFormatException("Multiple tIME chunks are not allowed.");
            var timeData = reader.ReadTimeChunkData();
            decoder.Time = timeData;
            reader.ReadAndValidateCrc(HeaderIds.TIME);
            return;
        }

        if (header.Id == HeaderIds.BKGD)
        {
            if (decoder.Bkgd.HasValue)
                throw new PngFormatException("Multiple bKGD chunks are not allowed.");
            if (m_SeenIdat)
                throw new PngFormatException("bKGD chunk must appear before IDAT.");
            var bkgdData = reader.ReadBkgdChunkData(header.ChunkSizeInBytes);
            decoder.Bkgd = bkgdData;
            reader.ReadAndValidateCrc(HeaderIds.BKGD);
            return;
        }

        if (header.Id == HeaderIds.EXIF)
        {
            if (decoder.Exif.HasValue)
                throw new PngFormatException("Multiple eXIf chunks are not allowed.");
            if (m_SeenIdat)
                throw new PngFormatException("eXIf chunk must appear before IDAT.");
            var exifData = reader.ReadExifChunkData(header.ChunkSizeInBytes);
            decoder.Exif = exifData;
            reader.ReadAndValidateCrc(HeaderIds.EXIF);
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
