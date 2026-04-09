using PngSharp.Spec;

namespace PngSharp.Decoder;

internal sealed class UnpackingRowReader : IRowReader
{
    private readonly byte m_BitDepth;
    private readonly byte[] m_PackedBuf;
    private readonly byte[] m_UnpackedBuf;

    public UnpackingRowReader(byte bitDepth, int maxPassScanlineByteWidth, int maxPassWidth)
    {
        m_BitDepth = bitDepth;
        m_PackedBuf = new byte[maxPassScanlineByteWidth];
        m_UnpackedBuf = new byte[maxPassWidth];
    }

    public Span<byte> ReadRow(Stream stream, int passWidth, int passScanlineByteWidth)
    {
        var packed = m_PackedBuf.AsSpan(0, passScanlineByteWidth);
        var unpacked = m_UnpackedBuf.AsSpan(0, passWidth);
        stream.ReadExactly(packed);
        BitDepthConverter.UnpackScanline(packed, unpacked, m_BitDepth, passWidth);
        return unpacked;
    }
}
