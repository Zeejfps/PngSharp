using System.IO.Compression;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Decoder.States;

internal sealed class DecodePixelDataState : IDecoderState
{
    private readonly PngDecoder m_Decoder;

    public DecodePixelDataState(PngDecoder decoder)
    {
        m_Decoder = decoder;
    }

    public void Execute()
    {
        var decoder = m_Decoder;
        var ihdr = decoder.IhdrChunkData;
        var compressedPixelData = decoder.CompressedPixelDataStream;
        compressedPixelData.Seek(0, SeekOrigin.Begin);
        using var decompressionStream = new ZLibStream(compressedPixelData, CompressionMode.Decompress);

        if (ihdr.InterlaceMethod == InterlaceMethod.Adam7)
            DecodeAdam7(decoder, ihdr, decompressionStream);
        else
            DecodeNonInterlaced(decoder, ihdr, decompressionStream);

        decoder.State = decoder.DoneState;
    }

    private static void DecodeNonInterlaced(PngDecoder decoder, IhdrChunkData ihdr, Stream decompressionStream)
    {
        var scanlineByteWidth = ihdr.GetScanlineByteWidth();
        var bytesPerPixel = ihdr.GetBytesPerPixel();
        var adaptiveFilter = new PngAdaptiveFilter(
            (int)ihdr.Height,
            scanlineByteWidth,
            bytesPerPixel
        );

        if (ihdr.BitDepth < 8)
        {
            using var packedStream = new MemoryStream();
            adaptiveFilter.Reverse(packedStream, decompressionStream);
            packedStream.Position = 0;

            var widthInPixels = (int)ihdr.Width;
            var heightInPixels = (int)ihdr.Height;
            const int stackAllocThreshold = 1024;
            var packedRow = scanlineByteWidth <= stackAllocThreshold
                ? stackalloc byte[scanlineByteWidth]
                : new byte[scanlineByteWidth];
            var unpackedRow = widthInPixels <= stackAllocThreshold
                ? stackalloc byte[widthInPixels]
                : new byte[widthInPixels];

            for (var y = 0; y < heightInPixels; y++)
            {
                packedStream.ReadExactly(packedRow);
                BitDepthConverter.UnpackScanline(packedRow, unpackedRow, ihdr.BitDepth, widthInPixels);
                decoder.PixelDataStream.Write(unpackedRow);
            }
        }
        else
        {
            adaptiveFilter.Reverse(decoder.PixelDataStream, decompressionStream);
        }
    }

    private static void DecodeAdam7(PngDecoder decoder, IhdrChunkData ihdr, Stream decompressionStream)
    {
        var width = (int)ihdr.Width;
        var height = (int)ihdr.Height;
        var bytesPerPixel = ihdr.GetBytesPerPixel();
        var bitsPerPixel = ihdr.GetBitsPerPixel();
        var isSubByte = ihdr.BitDepth < 8;

        // bytesPerPixel for the final (unpacked) image is always at least 1
        var finalBpp = isSubByte ? 1 : bytesPerPixel;
        var finalPixels = new byte[width * height * finalBpp];

        // Pass 7 is always the largest — pre-allocate row buffers once
        var maxPassWidth = Adam7.GetPassWidth(width, Adam7.PassCount - 1);
        var maxPassScanlineByteWidth = Adam7.GetPassScanlineByteWidth(maxPassWidth, bitsPerPixel);
        var maxRowByteWidth = maxPassWidth * bytesPerPixel;
        const int stackAllocThreshold = 1024;

        var packedRowBuf = isSubByte
            ? (maxPassScanlineByteWidth <= stackAllocThreshold ? stackalloc byte[maxPassScanlineByteWidth] : new byte[maxPassScanlineByteWidth])
            : Span<byte>.Empty;
        var unpackedRowBuf = isSubByte
            ? (maxPassWidth <= stackAllocThreshold ? stackalloc byte[maxPassWidth] : new byte[maxPassWidth])
            : Span<byte>.Empty;
        var rowBytesBuf = !isSubByte
            ? (maxRowByteWidth <= stackAllocThreshold ? stackalloc byte[maxRowByteWidth] : new byte[maxRowByteWidth])
            : Span<byte>.Empty;

        for (var pass = 0; pass < Adam7.PassCount; pass++)
        {
            var passWidth = Adam7.GetPassWidth(width, pass);
            var passHeight = Adam7.GetPassHeight(height, pass);
            if (passWidth == 0 || passHeight == 0)
                continue;

            var passScanlineByteWidth = Adam7.GetPassScanlineByteWidth(passWidth, bitsPerPixel);
            var filter = new PngAdaptiveFilter(passHeight, passScanlineByteWidth, bytesPerPixel);

            using var passStream = new MemoryStream();
            filter.Reverse(passStream, decompressionStream);
            passStream.Position = 0;

            var colStart = Adam7.GetColStart(pass);
            var colInc = Adam7.GetColInc(pass);
            var rowStart = Adam7.GetRowStart(pass);
            var rowInc = Adam7.GetRowInc(pass);

            if (isSubByte)
            {
                var packedRow = packedRowBuf[..passScanlineByteWidth];
                var unpackedRow = unpackedRowBuf[..passWidth];

                for (var r = 0; r < passHeight; r++)
                {
                    passStream.ReadExactly(packedRow);
                    BitDepthConverter.UnpackScanline(packedRow, unpackedRow, ihdr.BitDepth, passWidth);

                    var destRow = rowStart + r * rowInc;
                    for (var c = 0; c < passWidth; c++)
                    {
                        var destCol = colStart + c * colInc;
                        finalPixels[destRow * width + destCol] = unpackedRow[c];
                    }
                }
            }
            else
            {
                var rowBytes = rowBytesBuf[..(passWidth * bytesPerPixel)];

                for (var r = 0; r < passHeight; r++)
                {
                    passStream.ReadExactly(rowBytes);

                    var destRow = rowStart + r * rowInc;
                    for (var c = 0; c < passWidth; c++)
                    {
                        var destCol = colStart + c * colInc;
                        var srcOffset = c * bytesPerPixel;
                        var dstOffset = (destRow * width + destCol) * bytesPerPixel;
                        rowBytes.Slice(srcOffset, bytesPerPixel).CopyTo(finalPixels.AsSpan(dstOffset, bytesPerPixel));
                    }
                }
            }
        }

        decoder.PixelDataStream.Write(finalPixels);
    }
}
