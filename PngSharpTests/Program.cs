using System.Text;
using PngSharp;

var decodedPng = Png.DecodeFromFile("Assets/sprite_atlas.png");

SaveToPAM("test.pam", decodedPng.PixelData, decodedPng.Width, decodedPng.Height);

string ToHexString(ReadOnlySpan<byte> bytes)
{
    return string.Concat(bytes.ToArray().Select(b => b.ToString("X2")));
}

void SaveToPAM(string filename, byte[] rgbaData, int width, int height)
{
    using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
    using (BinaryWriter bw = new BinaryWriter(fs))
    {
        // Write PAM header
        bw.Write(Encoding.ASCII.GetBytes("P7\n"));
        bw.Write(Encoding.ASCII.GetBytes($"WIDTH {width}\n"));
        bw.Write(Encoding.ASCII.GetBytes($"HEIGHT {height}\n"));
        bw.Write(Encoding.ASCII.GetBytes("DEPTH 4\n"));
        bw.Write(Encoding.ASCII.GetBytes("MAXVAL 255\n"));
        bw.Write(Encoding.ASCII.GetBytes("TUPLTYPE RGB_ALPHA\n"));
        bw.Write(Encoding.ASCII.GetBytes("ENDHDR\n"));

        // Write image data
        bw.Write(rgbaData);
    }
}
