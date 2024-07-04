namespace PngSharp.Decoder;

internal class PngScanLineDecoder
{
    private readonly Stream m_Stream;
    private readonly byte[] m_CurrentFilteredScanLine;
    private byte[] m_PreviousUnfilteredScanLine;
    private byte[] m_CurrentUnfilteredScanLine;

    private readonly int m_BytesPerPixel;
    private bool m_IsFirstScanLine;
    
    public PngScanLineDecoder(PngSpec.IhdrChunkData imageData, Stream stream)
    {
        m_Stream = stream;
        var imageWidth = imageData.Width;
        m_BytesPerPixel = 4;
        var stride = imageWidth * m_BytesPerPixel;
        m_CurrentFilteredScanLine = new byte[stride + 1];
        m_PreviousUnfilteredScanLine = new byte[stride];
        m_CurrentUnfilteredScanLine = new byte[stride];
    }

    public void DecodeScanlineTo(Stream outStream)
    {
        m_Stream.ReadExactly(m_CurrentFilteredScanLine);
        var type = (PngSpec.AdaptiveFilteringType)m_CurrentFilteredScanLine[0];
        
        Func<byte, int, byte> reverseFilterFunc = type switch
        {
            PngSpec.AdaptiveFilteringType.None => ReverseNone,
            PngSpec.AdaptiveFilteringType.Sub => ReverseSub,
            PngSpec.AdaptiveFilteringType.Up => ReverseUp,
            PngSpec.AdaptiveFilteringType.Average => ReverseAverage,
            PngSpec.AdaptiveFilteringType.Paeth => ReversePaeth,
            _ => throw new ArgumentOutOfRangeException()
        };

        for (var i = 1; i < m_CurrentFilteredScanLine.Length; i++)
        {
            var x = m_CurrentFilteredScanLine[i];
            var currByteIndex = i - 1;
            var value = reverseFilterFunc.Invoke(x, currByteIndex);
            m_CurrentUnfilteredScanLine[currByteIndex] = value;
        }
        
        outStream.Write(m_CurrentUnfilteredScanLine);
        
        // Swap the pointers here so we don't have to copy the data over
        (m_PreviousUnfilteredScanLine, m_CurrentUnfilteredScanLine) = (m_CurrentUnfilteredScanLine, m_PreviousUnfilteredScanLine);

        m_IsFirstScanLine = false;
    }

    private byte ReverseNone(byte x, int currByteIndex)
    {
        return x;
    }
    
    private byte ReverseSub(byte x, int currByteIndex)
    {
        var leftValue = GetLeftValue(currByteIndex);
        return (byte)(x + leftValue);
    }
    
    private byte ReverseUp(byte x, int currByteIndex)
    {
        var upValue = GetUpValue(currByteIndex);
        return (byte)(x + upValue);
    }

    private byte ReverseAverage(byte x, int currByteIndex)
    {
        var reconValue = (byte)(GetLeftValue(currByteIndex) + GetUpValue(currByteIndex) * 0.5);
        return (byte)(x + reconValue);
    }

    private byte ReversePaeth(byte x, int currByteIndex)
    {
        var a = GetLeftValue(currByteIndex);
        var b = GetUpValue(currByteIndex );
        var c = GetUpLeftByteValue(currByteIndex);
        return (byte)(x + PaethPredictor(a, b, c));
    }

    private byte GetUpLeftByteValue(int currByteIndex)
    {
        if (m_IsFirstScanLine || currByteIndex < m_BytesPerPixel)
            return 0;
        return m_PreviousUnfilteredScanLine[currByteIndex - m_BytesPerPixel];
    }

    private byte GetLeftValue(int currByteIndex)
    {
        if (currByteIndex < m_BytesPerPixel)
            return 0;
        return m_CurrentUnfilteredScanLine[currByteIndex - m_BytesPerPixel];
    }

    private byte GetUpValue(int currByteIndex)
    {
        if (m_IsFirstScanLine)
            return 0;
        return m_PreviousUnfilteredScanLine[currByteIndex];
    }

    private byte PaethPredictor(byte a, byte b, byte c)
    {
        var p = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);

        if (pa <= pb && pa <= pc)
            return a;
        if (pb <= pc)
            return b;
        return c;
    }
}