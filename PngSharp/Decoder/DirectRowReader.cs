namespace PngSharp.Decoder;

internal sealed class DirectRowReader : IRowReader
{
    private readonly byte[] m_RowBuf;
    private readonly int m_BytesPerPixel;

    public DirectRowReader(int maxPassWidth, int bytesPerPixel)
    {
        m_BytesPerPixel = bytesPerPixel;
        m_RowBuf = new byte[maxPassWidth * bytesPerPixel];
    }

    public Span<byte> ReadRow(Stream stream, int passWidth, int passScanlineByteWidth)
    {
        var row = m_RowBuf.AsSpan(0, passWidth * m_BytesPerPixel);
        stream.ReadExactly(row);
        return row;
    }
}
