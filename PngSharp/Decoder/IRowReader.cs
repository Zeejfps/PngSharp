namespace PngSharp.Decoder;

internal interface IRowReader
{
    Span<byte> ReadRow(Stream stream, int passWidth, int passScanlineByteWidth);
}
