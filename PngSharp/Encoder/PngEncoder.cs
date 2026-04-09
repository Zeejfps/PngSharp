using System.IO.Compression;
using PngSharp.Api;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;

namespace PngSharp.Encoder;

internal sealed class PngEncoder : IDisposable, IAsyncDisposable
{
    private readonly ILogger m_Logger;
    private readonly IRawPng m_Png;
    private readonly PngWriter m_PngWriter;
    private readonly PngAdaptiveFilter m_AdaptiveFilter;

    public PngEncoder(IRawPng png, PngWriter writer, ILogger logger)
    {
        m_Png = png;
        m_PngWriter = writer;
        m_Logger = logger;
        var ihdr = m_Png.Ihdr;
        m_AdaptiveFilter = new PngAdaptiveFilter((int)ihdr.Height, ihdr.GetScanlineByteWidth(), ihdr.GetBytesPerPixel());
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

        foreach (var textChunk in png.TextChunks)
        {
            m_Logger.Debug($"Writing text chunk: {textChunk.Keyword}");
            writer.WriteTextChunk(textChunk);
        }

        m_Logger.Debug($"Uncompressed Size: {png.PixelData.Length} bytes");
        using var pixelDataStream = new MemoryStream(png.PixelData);
        using var compressedDataStream = new MemoryStream();
        using (var compressionStream = new ZLibStream(compressedDataStream, CompressionLevel.Optimal, true))
        {
            EncodePixels(compressionStream, pixelDataStream);
        }
        m_Logger.Debug($"Compressed Size: {compressedDataStream.Length} bytes");
        writer.WriteIDATChunk(compressedDataStream.ToArray());
        
        writer.WriteIENDChunk();
    }
    
    private void EncodePixels(Stream outputStream, Stream inputStream)
    {
        var ihdr = m_Png.Ihdr;
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
            m_AdaptiveFilter.Apply(outputStream, packedStream);
        }
        else
        {
            m_AdaptiveFilter.Apply(outputStream, inputStream);
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