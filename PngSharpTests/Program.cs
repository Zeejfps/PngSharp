using System.Text;
using PngSharp.Api;

var decodedPng = PngApi.DecodeFromFile("Assets/sprite_atlas_128x64.png");

PngApi.EncodeToFile(decodedPng, "test_64x64.png");

Console.WriteLine(decodedPng.ColorType);

SaveToPAM("test.pam", decodedPng.PixelData, decodedPng.Width, decodedPng.Height);

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