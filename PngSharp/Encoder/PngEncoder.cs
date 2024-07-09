using System.IO.Compression;
using PngSharp.Api;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Encoder;

internal sealed class PngEncoder : IDisposable, IAsyncDisposable
{
    private readonly IDecodedPng m_Png;
    private readonly PngWriter m_PngWriter;
    private readonly PngAdaptiveFilter m_AdaptiveFilter;

    public PngEncoder(IDecodedPng png, Stream stream)
    {
        m_Png = png;
        m_PngWriter = new PngWriter(stream);
        m_AdaptiveFilter = new PngAdaptiveFilter(m_Png.Width, m_Png.Height, m_Png.BytesPerPixel);
    }
    
    public void Encode()
    {
        var png = m_Png;
        var writer = m_PngWriter;
        
        writer.WriteSignature();
        writer.WriteIHDRChunk(new IhdrChunkData
        {
            Width = (uint)png.Width,
            Height = (uint)png.Height,
            CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
            FilterMethod = FilterMethod.AdaptiveFiltering,
            ColorType = png.ColorType,
            InterlaceMethod = InterlaceMethod.None, // TODO: Make dynamic?
            BitDepth = 8, // TODO: Make dynamic
        });

        
        if (png.Srgb.TryGetData(out var srgbChunkData))
        {
            Console.WriteLine("Has SRGB Data");
            writer.WriteSRGBChunk(srgbChunkData);
        }
        
        if (png.Gama.TryGetData(out var gammaChunkData))
        {
            Console.WriteLine($"Has Gama data: {gammaChunkData}");
            writer.WriteGAMAChunk(gammaChunkData);
        }

        if (png.Phys.TryGetData(out var physChunkData))
        {
            Console.WriteLine($"Has Phys Data: {physChunkData}");
            writer.WritePHYSChunk(physChunkData);
        }
        
        Console.WriteLine($"Uncompressed Size: {png.PixelData.Length} bytes");
        using var pixelDataStream = new MemoryStream(png.PixelData);
        using var compressedDataStream = new MemoryStream();
        using (var compressionStream = new ZLibStream(compressedDataStream, CompressionLevel.Optimal, true))
        {
            EncodePixels(compressionStream, pixelDataStream);
        }
        Console.WriteLine($"Compressed Size: {compressedDataStream.Length} bytes");
        writer.WriteIDATChunk(compressedDataStream.ToArray());
        
        writer.WriteIENDChunk();
    }
    
    private void EncodePixels(Stream outputStream, Stream inputStream)
    {
        m_AdaptiveFilter.Apply(outputStream, inputStream);
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