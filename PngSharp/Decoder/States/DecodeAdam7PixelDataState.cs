using System.IO.Compression;
using PngSharp.Spec;
using PngSharp.Spec.AdaptiveFilter;

namespace PngSharp.Decoder.States;

internal sealed class DecodeAdam7PixelDataState : IDecoderState
{
    private readonly PngDecoder m_Decoder;

    public DecodeAdam7PixelDataState(PngDecoder decoder)
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

        var width = (int)ihdr.Width;
        var height = (int)ihdr.Height;
        var bytesPerPixel = ihdr.GetBytesPerPixel();
        var bitsPerPixel = ihdr.GetBitsPerPixel();
        var isSubByte = ihdr.BitDepth < 8;

        // After unpacking, sub-byte pixels are 1 byte each
        var bpp = isSubByte ? 1 : bytesPerPixel;
        var finalPixels = new byte[width * height * bpp];

        var maxPassWidth = Adam7.GetPassWidth(width, Adam7.PassCount - 1);
        var maxPassScanlineByteWidth = Adam7.GetPassScanlineByteWidth(maxPassWidth, bitsPerPixel);

        IRowReader rowReader = isSubByte
            ? new UnpackingRowReader(ihdr.BitDepth, maxPassScanlineByteWidth, maxPassWidth)
            : new DirectRowReader(maxPassWidth, bytesPerPixel);

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

            for (var r = 0; r < passHeight; r++)
            {
                var row = rowReader.ReadRow(passStream, passWidth, passScanlineByteWidth);
                var destRow = rowStart + r * rowInc;
                for (var c = 0; c < passWidth; c++)
                {
                    var destCol = colStart + c * colInc;
                    var srcOffset = c * bpp;
                    var dstOffset = (destRow * width + destCol) * bpp;
                    row.Slice(srcOffset, bpp).CopyTo(finalPixels.AsSpan(dstOffset, bpp));
                }
            }
        }

        decoder.PixelDataStream.Write(finalPixels);
        decoder.State = decoder.DoneState;
    }
}
