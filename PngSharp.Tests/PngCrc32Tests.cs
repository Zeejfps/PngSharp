using System.Text;
using PngSharp.Spec;
using Xunit;

namespace PngSharp.Tests;

public class PngCrc32Tests
{
    [Fact]
    public void Update_SingleByte_ProducesKnownCrc()
    {
        var crc = new PngCrc32();
        crc.Reset();
        crc.Update(0x00);
        Assert.Equal(0xD202EF8Du, crc.Value);
    }

    [Fact]
    public void Update_AsciiCheckString_ProducesKnownCrc()
    {
        var crc = new PngCrc32();
        crc.Reset();
        var data = Encoding.ASCII.GetBytes("123456789");
        foreach (var b in data)
            crc.Update(b);
        Assert.Equal(0xCBF43926u, crc.Value);
    }

    [Fact]
    public void Update_SpanOverload_MatchesByteByByte()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0xFF, 0xAB, 0xCD };

        var crc1 = new PngCrc32();
        crc1.Reset();
        foreach (var b in data)
            crc1.Update(b);

        var crc2 = new PngCrc32();
        crc2.Reset();
        crc2.Update(data);

        Assert.Equal(crc1.Value, crc2.Value);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var data = Encoding.ASCII.GetBytes("123456789");

        var crc = new PngCrc32();
        crc.Reset();
        crc.Update(data);
        var first = crc.Value;

        // Update with garbage to change internal state
        crc.Update(0xFF);

        // Reset and recompute — should match
        crc.Reset();
        crc.Update(data);
        Assert.Equal(first, crc.Value);
    }
}
