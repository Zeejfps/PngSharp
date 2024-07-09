namespace PngSharp.Spec;

/// <summary>
/// 32-bit Cyclic Redundancy Code used by the PNG for checking the data is intact.
/// </summary>
public sealed class PngCrc32
{
    private const uint Polynomial = 0xEDB88320;
    private static readonly uint[] s_Lookup;

    static PngCrc32()
    {
        s_Lookup = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var value = i;
            for (var j = 0; j < 8; ++j)
            {
                if ((value & 1) != 0)
                    value = (value >> 1) ^ Polynomial;
                else
                    value >>= 1;
            }

            s_Lookup[i] = value;
        }
    }

    public uint Value => m_Value ^ uint.MaxValue; 
    
    private uint m_Value;

    public void Reset()
    {
        m_Value = uint.MaxValue;
    }

    public void Update(byte b)
    {
        var crc32 = m_Value;
        var index = (crc32 ^ b) & 0xFF;
        crc32 = (crc32 >> 8) ^ s_Lookup[index];
        m_Value = crc32;
    }
    
    public void Update(ReadOnlySpan<byte> data)
    {
        var crc32 = m_Value;
        foreach (var b in data)
        {
            var index = (crc32 ^ b) & 0xFF;
            crc32 = (crc32 >> 8) ^ s_Lookup[index];
        }
        m_Value = crc32;
    }
}