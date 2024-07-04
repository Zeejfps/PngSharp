using PngSharp;

class PngImageDataContainer : IPngImageDataContainer
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] PixelData { get; private set; }
    
    public void Load(int width, int height, Stream pixelDataStream)
    {
        Width = width;
        Height = height;
        PixelData = new byte[width * height * 4];
        pixelDataStream.Read(PixelData);
    }
}