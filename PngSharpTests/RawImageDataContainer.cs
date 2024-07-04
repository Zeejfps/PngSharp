using PngSharp;

class PngImageDataContainer : IPngImageDataContainer
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] PixelData { get; private set; }
    
    public void Load(int width, int height, byte[] pixelData)
    {
        Width = width;
        Height = height;
        PixelData = pixelData;
    }
}