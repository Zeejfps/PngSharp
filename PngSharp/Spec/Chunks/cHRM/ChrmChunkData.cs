namespace PngSharp.Spec.Chunks.cHRM;

public readonly record struct ChrmChunkData
{
    private const double ScaleFactor = 100000.0;

    public uint WhitePointX { get; init; }
    public uint WhitePointY { get; init; }
    public uint RedX { get; init; }
    public uint RedY { get; init; }
    public uint GreenX { get; init; }
    public uint GreenY { get; init; }
    public uint BlueX { get; init; }
    public uint BlueY { get; init; }

    public ChrmChunkContent DecodeContent()
    {
        return new ChrmChunkContent
        {
            WhitePointX = WhitePointX / ScaleFactor,
            WhitePointY = WhitePointY / ScaleFactor,
            RedX = RedX / ScaleFactor,
            RedY = RedY / ScaleFactor,
            GreenX = GreenX / ScaleFactor,
            GreenY = GreenY / ScaleFactor,
            BlueX = BlueX / ScaleFactor,
            BlueY = BlueY / ScaleFactor,
        };
    }

    public static ChrmChunkData Create(ChrmChunkContent content)
    {
        return new ChrmChunkData
        {
            WhitePointX = (uint)Math.Round(content.WhitePointX * ScaleFactor),
            WhitePointY = (uint)Math.Round(content.WhitePointY * ScaleFactor),
            RedX = (uint)Math.Round(content.RedX * ScaleFactor),
            RedY = (uint)Math.Round(content.RedY * ScaleFactor),
            GreenX = (uint)Math.Round(content.GreenX * ScaleFactor),
            GreenY = (uint)Math.Round(content.GreenY * ScaleFactor),
            BlueX = (uint)Math.Round(content.BlueX * ScaleFactor),
            BlueY = (uint)Math.Round(content.BlueY * ScaleFactor),
        };
    }
}
