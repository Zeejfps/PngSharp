using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.tIME;
using PngSharp.Spec.Chunks.tRNS;

namespace PngSharp.Spec;

internal sealed class RawPngBuilder : IRawPngBuilder
{
    private IhdrChunkData? m_Ihdr;
    private byte[]? m_PixelData;
    private PlteChunkData? m_Plte;
    private TrnsChunkData? m_Trns;
    private SrgbChunkData? m_Srgb;
    private GammaChunkData? m_Gama;
    private PhysChunkData? m_Phys;
    private ChrmChunkData? m_Chrm;
    private TimeChunkData? m_Time;
    private BkgdChunkData? m_Bkgd;
    private readonly List<TextChunk> m_TxtChunks = [];
    private readonly List<ZTextChunk> m_ZTxtChunks = [];
    private readonly List<ITextChunk> m_ITxtChunks = [];

    public IRawPngBuilder WithIhdr(IhdrChunkData ihdr)
    {
        m_Ihdr = ihdr;
        return this;
    }

    public IRawPngBuilder WithPixelData(byte[] pixels)
    {
        m_PixelData = pixels;
        return this;
    }

    public IRawPngBuilder WithPlte(PlteChunkData plte)
    {
        m_Plte = plte;
        return this;
    }

    public IRawPngBuilder WithTrns(TrnsChunkData trns)
    {
        m_Trns = trns;
        return this;
    }

    public IRawPngBuilder WithSrgb(SrgbChunkData srgb)
    {
        m_Srgb = srgb;
        return this;
    }

    public IRawPngBuilder WithGama(GammaChunkData gama)
    {
        m_Gama = gama;
        return this;
    }

    public IRawPngBuilder WithPhys(PhysChunkData phys)
    {
        m_Phys = phys;
        return this;
    }

    public IRawPngBuilder WithChrm(ChrmChunkData chrm)
    {
        m_Chrm = chrm;
        return this;
    }

    public IRawPngBuilder WithTime(TimeChunkData time)
    {
        m_Time = time;
        return this;
    }

    public IRawPngBuilder WithBkgd(BkgdChunkData bkgd)
    {
        m_Bkgd = bkgd;
        return this;
    }

    public IRawPngBuilder WithTxtChunk(TextChunk textChunk)
    {
        m_TxtChunks.Add(textChunk);
        return this;
    }

    public IRawPngBuilder WithZTxtChunk(ZTextChunk textChunk)
    {
        m_ZTxtChunks.Add(textChunk);
        return this;
    }

    public IRawPngBuilder WithITxtChunk(ITextChunk textChunk)
    {
        m_ITxtChunks.Add(textChunk);
        return this;
    }

    public IRawPng Build()
    {
        if (m_Ihdr is null)
            throw new InvalidOperationException("Ihdr is required.");
        if (m_PixelData is null)
            throw new InvalidOperationException("PixelData is required.");

        var ihdr = m_Ihdr.Value;

        if (ihdr.Width == 0)
            throw new InvalidOperationException("Width must be greater than zero.");
        if (ihdr.Height == 0)
            throw new InvalidOperationException("Height must be greater than zero.");

        ValidateBitDepth(ihdr.BitDepth, ihdr.ColorType);
        ValidatePlte(ihdr, m_Plte);
        ValidateTrns(ihdr, m_Trns, m_Plte);
        ValidateBkgd(ihdr, m_Bkgd);
        ValidateTime(m_Time);
        ValidateTextKeywords(m_TxtChunks, m_ZTxtChunks, m_ITxtChunks);

        var expectedLength = (int)ihdr.Width * (int)ihdr.Height * ihdr.GetBytesPerPixel();
        if (m_PixelData.Length != expectedLength)
            throw new InvalidOperationException(
                $"PixelData length {m_PixelData.Length} does not match expected length {expectedLength} " +
                $"for a {ihdr.Width}x{ihdr.Height} image with {ihdr.ColorType} color type and {ihdr.BitDepth}-bit depth.");

        return new RawPng
        {
            Ihdr = ihdr,
            PixelData = m_PixelData,
            Plte = m_Plte,
            Trns = m_Trns,
            Srgb = m_Srgb,
            Gama = m_Gama,
            Phys = m_Phys,
            Chrm = m_Chrm,
            Time = m_Time,
            Bkgd = m_Bkgd,
            TxtChunks = m_TxtChunks,
            ZTxtChunks = m_ZTxtChunks,
            ITxtChunks = m_ITxtChunks,
        };
    }

    private static void ValidatePlte(IhdrChunkData ihdr, PlteChunkData? plte)
    {
        if (ihdr.ColorType is ColorType.Grayscale or ColorType.GrayscaleWithAlpha && plte.HasValue)
            throw new InvalidOperationException($"PLTE chunk is forbidden for {ihdr.ColorType}.");

        if (ihdr.ColorType == ColorType.IndexedColor && !plte.HasValue)
            throw new InvalidOperationException("PLTE chunk is required for IndexedColor.");

        if (plte.HasValue)
        {
            var entries = plte.Value.Entries;
            if (entries.Length == 0 || entries.Length % 3 != 0)
                throw new InvalidOperationException(
                    $"PLTE data length {entries.Length} must be a positive multiple of 3.");

            var maxEntries = 1 << ihdr.BitDepth;
            if (plte.Value.EntryCount > maxEntries)
                throw new InvalidOperationException(
                    $"PLTE has {plte.Value.EntryCount} entries but bit depth {ihdr.BitDepth} allows at most {maxEntries}.");
        }
    }

    private static void ValidateTrns(IhdrChunkData ihdr, TrnsChunkData? trns, PlteChunkData? plte)
    {
        if (!trns.HasValue)
            return;

        if (ihdr.ColorType is ColorType.GrayscaleWithAlpha or ColorType.TrueColorWithAlpha)
            throw new InvalidOperationException($"tRNS chunk is forbidden for {ihdr.ColorType}.");

        var data = trns.Value.Data;
        switch (ihdr.ColorType)
        {
            case ColorType.Grayscale:
                if (data.Length != 2)
                    throw new InvalidOperationException(
                        $"tRNS data length for Grayscale must be 2, got {data.Length}.");
                break;
            case ColorType.TrueColor:
                if (data.Length != 6)
                    throw new InvalidOperationException(
                        $"tRNS data length for TrueColor must be 6, got {data.Length}.");
                break;
            case ColorType.IndexedColor:
                var maxEntries = plte?.EntryCount ?? 0;
                if (data.Length > maxEntries)
                    throw new InvalidOperationException(
                        $"tRNS has {data.Length} entries but palette has only {maxEntries}.");
                break;
        }
    }

    private static void ValidateTextKeywords(
        List<TextChunk> text,
        List<ZTextChunk> compressed,
        List<ITextChunk> international)
    {
        foreach (var chunk in text)
            ValidateKeyword(chunk.Keyword);
        foreach (var chunk in compressed)
            ValidateKeyword(chunk.Keyword);
        foreach (var chunk in international)
            ValidateKeyword(chunk.Keyword);
    }

    private static void ValidateKeyword(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
            throw new InvalidOperationException("Text chunk keyword must not be empty.");
        if (keyword.Length > 79)
            throw new InvalidOperationException(
                $"Text chunk keyword '{keyword}' exceeds maximum length of 79 bytes.");
    }

    private static void ValidateBkgd(IhdrChunkData ihdr, BkgdChunkData? bkgd)
    {
        if (!bkgd.HasValue)
            return;

        var data = bkgd.Value.Data;
        switch (ihdr.ColorType)
        {
            case ColorType.Grayscale:
            case ColorType.GrayscaleWithAlpha:
                if (data.Length != 2)
                    throw new InvalidOperationException(
                        $"bKGD data length for {ihdr.ColorType} must be 2, got {data.Length}.");
                break;
            case ColorType.TrueColor:
            case ColorType.TrueColorWithAlpha:
                if (data.Length != 6)
                    throw new InvalidOperationException(
                        $"bKGD data length for {ihdr.ColorType} must be 6, got {data.Length}.");
                break;
            case ColorType.IndexedColor:
                if (data.Length != 1)
                    throw new InvalidOperationException(
                        $"bKGD data length for IndexedColor must be 1, got {data.Length}.");
                break;
        }
    }

    private static void ValidateTime(TimeChunkData? time)
    {
        if (!time.HasValue)
            return;

        var t = time.Value;
        if (t.Month is < 1 or > 12)
            throw new InvalidOperationException($"tIME month must be 1-12, got {t.Month}.");
        if (t.Day is < 1 or > 31)
            throw new InvalidOperationException($"tIME day must be 1-31, got {t.Day}.");
        if (t.Hour > 23)
            throw new InvalidOperationException($"tIME hour must be 0-23, got {t.Hour}.");
        if (t.Minute > 59)
            throw new InvalidOperationException($"tIME minute must be 0-59, got {t.Minute}.");
        if (t.Second > 60)
            throw new InvalidOperationException($"tIME second must be 0-60, got {t.Second}.");
    }

    private static void ValidateBitDepth(byte bitDepth, ColorType colorType)
    {
        byte[] allowed = colorType switch
        {
            ColorType.Grayscale => [1, 2, 4, 8, 16],
            ColorType.TrueColor => [8, 16],
            ColorType.IndexedColor => [1, 2, 4, 8],
            ColorType.GrayscaleWithAlpha => [8, 16],
            ColorType.TrueColorWithAlpha => [8, 16],
            _ => throw new InvalidOperationException($"Unknown ColorType: {colorType}."),
        };

        if (Array.IndexOf(allowed, bitDepth) < 0)
            throw new InvalidOperationException(
                $"BitDepth {bitDepth} is not valid for {colorType}. Allowed values: {string.Join(", ", allowed)}.");
    }
}
