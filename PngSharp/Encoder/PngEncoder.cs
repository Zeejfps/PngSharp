using System.IO.Compression;
using PngSharp.Api;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;
using PngSharp.Spec.Chunks.IHDR;


namespace PngSharp.Encoder;

internal sealed class PngEncoder : IDisposable, IAsyncDisposable
{
    private readonly ILogger m_Logger;
    private readonly IRawPng m_Png;
    private readonly PngWriter m_PngWriter;

    public PngEncoder(IRawPng png, PngWriter writer, ILogger logger)
    {
        m_Png = png;
        m_PngWriter = writer;
        m_Logger = logger;
    }

    public void Encode()
    {
        var png = m_Png;
        var writer = m_PngWriter;

        writer.WriteSignature();
        writer.WriteIHDRChunk(png.Ihdr);

        if (png.Srgb.HasValue)
        {
            m_Logger.Debug("Has SRGB Data");
            writer.WriteSRGBChunk(png.Srgb.Value);
        }

        if (png.Gama.HasValue)
        {
            m_Logger.Debug($"Has Gama data: {png.Gama.Value}");
            writer.WriteGAMAChunk(png.Gama.Value);
        }

        if (png.Chrm.HasValue)
        {
            m_Logger.Debug("Has cHRM Data");
            writer.WriteCHRMChunk(png.Chrm.Value);
        }

        if (png.Phys.HasValue)
        {
            m_Logger.Debug($"Has Phys Data: {png.Phys.Value}");
            writer.WritePHYSChunk(png.Phys.Value);
        }

        if (png.Plte.HasValue)
        {
            m_Logger.Debug($"Has PLTE Data: {png.Plte.Value.EntryCount} entries");
            writer.WritePLTEChunk(png.Plte.Value);
        }

        if (png.Trns.HasValue)
        {
            m_Logger.Debug($"Has tRNS Data: {png.Trns.Value.Data.Length} bytes");
            writer.WriteTRNSChunk(png.Trns.Value);
        }

        if (png.Bkgd.HasValue)
        {
            m_Logger.Debug($"Has bKGD Data: {png.Bkgd.Value.Data.Length} bytes");
            writer.WriteBKGDChunk(png.Bkgd.Value);
        }

        foreach (var textChunk in png.TxtChunks)
            writer.WriteTxtChunk(textChunk);
        foreach (var textChunk in png.ZTxtChunks)
            writer.WriteZTxtChunk(textChunk);
        foreach (var textChunk in png.ITxtChunks)
            writer.WriteITxtChunk(textChunk);

        if (png.Time.HasValue)
        {
            m_Logger.Debug("Has tIME Data");
            writer.WriteTIMEChunk(png.Time.Value);
        }

        m_Logger.Debug($"Uncompressed Size: {png.PixelData.Length} bytes");
        using var pixelDataStream = new MemoryStream(png.PixelData);
        using var compressedDataStream = new MemoryStream();
        using (var compressionStream = new ZLibStream(compressedDataStream, CompressionLevel.Optimal, true))
        {
            if (png.Ihdr.InterlaceMethod == InterlaceMethod.Adam7)
                EncodeAdam7Pixels(compressionStream, pixelDataStream);
            else
                EncodePixels(compressionStream, pixelDataStream);
        }
        m_Logger.Debug($"Compressed Size: {compressedDataStream.Length} bytes");
        WriteIdatChunks(writer, compressedDataStream);

        writer.WriteIENDChunk();
    }

    private const int MaxIdatChunkSize = 8192;

    private static void WriteIdatChunks(PngWriter writer, MemoryStream compressedDataStream)
    {
        var data = compressedDataStream.GetBuffer().AsSpan(0, (int)compressedDataStream.Length);
        while (data.Length > 0)
        {
            var chunkSize = Math.Min(data.Length, MaxIdatChunkSize);
            writer.WriteIDATChunk(data[..chunkSize]);
            data = data[chunkSize..];
        }
    }

    private void EncodePixels(Stream outputStream, Stream inputStream)
    {
        var ihdr = m_Png.Ihdr;
        var adaptiveFilter = new PngAdaptiveFilter((int)ihdr.Height, ihdr.GetScanlineByteWidth(), ihdr.GetBytesPerPixel());

        if (ihdr.BitDepth < 8)
        {
            using var packedStream = new MemoryStream();
            var scanlineByteWidth = ihdr.GetScanlineByteWidth();
            var widthInPixels = (int)ihdr.Width;
            var heightInPixels = (int)ihdr.Height;
            const int stackAllocThreshold = 1024;
            var unpackedRow = widthInPixels <= stackAllocThreshold
                ? stackalloc byte[widthInPixels]
                : new byte[widthInPixels];
            var packedRow = scanlineByteWidth <= stackAllocThreshold
                ? stackalloc byte[scanlineByteWidth]
                : new byte[scanlineByteWidth];

            for (var y = 0; y < heightInPixels; y++)
            {
                inputStream.ReadExactly(unpackedRow);
                BitDepthConverter.PackScanline(unpackedRow, packedRow, ihdr.BitDepth, widthInPixels);
                packedStream.Write(packedRow);
            }
            packedStream.Position = 0;
            adaptiveFilter.Apply(outputStream, packedStream);
        }
        else
        {
            adaptiveFilter.Apply(outputStream, inputStream);
        }
    }

    private void EncodeAdam7Pixels(Stream outputStream, Stream inputStream)
    {
        var ihdr = m_Png.Ihdr;
        var width = (int)ihdr.Width;
        var height = (int)ihdr.Height;
        var bytesPerPixel = ihdr.GetBytesPerPixel();
        var bitsPerPixel = ihdr.GetBitsPerPixel();
        var isSubByte = ihdr.BitDepth < 8;

        var finalBpp = isSubByte ? 1 : bytesPerPixel;
        var allPixels = new byte[width * height * finalBpp];
        inputStream.ReadExactly(allPixels);

        var maxPassWidth = Adam7.GetPassWidth(width, Adam7.PassCount - 1);
        var maxPassScanlineByteWidth = Adam7.GetPassScanlineByteWidth(maxPassWidth, bitsPerPixel);
        var unpackedRowBuf = isSubByte ? new byte[maxPassWidth] : [];
        var packedRowBuf = isSubByte ? new byte[maxPassScanlineByteWidth] : [];
        var rowBytesBuf = !isSubByte ? new byte[maxPassWidth * bytesPerPixel] : [];

        for (var pass = 0; pass < Adam7.PassCount; pass++)
        {
            var passWidth = Adam7.GetPassWidth(width, pass);
            var passHeight = Adam7.GetPassHeight(height, pass);
            if (passWidth == 0 || passHeight == 0)
                continue;

            var passScanlineByteWidth = Adam7.GetPassScanlineByteWidth(passWidth, bitsPerPixel);
            var colStart = Adam7.GetColStart(pass);
            var colInc = Adam7.GetColInc(pass);
            var rowStart = Adam7.GetRowStart(pass);
            var rowInc = Adam7.GetRowInc(pass);

            using var passStream = new MemoryStream();

            if (isSubByte)
            {
                var unpackedRow = unpackedRowBuf.AsSpan(0, passWidth);
                var packedRow = packedRowBuf.AsSpan(0, passScanlineByteWidth);

                for (var r = 0; r < passHeight; r++)
                {
                    var srcRow = rowStart + r * rowInc;
                    for (var c = 0; c < passWidth; c++)
                    {
                        var srcCol = colStart + c * colInc;
                        unpackedRow[c] = allPixels[srcRow * width + srcCol];
                    }
                    BitDepthConverter.PackScanline(unpackedRow, packedRow, ihdr.BitDepth, passWidth);
                    passStream.Write(packedRow);
                }
            }
            else
            {
                var rowBytes = rowBytesBuf.AsSpan(0, passWidth * bytesPerPixel);

                for (var r = 0; r < passHeight; r++)
                {
                    var srcRow = rowStart + r * rowInc;
                    for (var c = 0; c < passWidth; c++)
                    {
                        var srcCol = colStart + c * colInc;
                        var srcOffset = (srcRow * width + srcCol) * bytesPerPixel;
                        var dstOffset = c * bytesPerPixel;
                        allPixels.AsSpan(srcOffset, bytesPerPixel).CopyTo(rowBytes.Slice(dstOffset, bytesPerPixel));
                    }
                    passStream.Write(rowBytes);
                }
            }

            passStream.Position = 0;
            var filter = new PngAdaptiveFilter(passHeight, passScanlineByteWidth, bytesPerPixel);
            filter.Apply(outputStream, passStream);
        }
    }

    public void Dispose()
    {
        m_PngWriter.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await m_PngWriter.DisposeAsync();
    }
}
