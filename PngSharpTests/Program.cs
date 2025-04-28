using PngSharp.Api;

Png.Logger = new ConsoleLogger();
var decodedPng = Png.DecodeFromFile("Assets/sprite_atlas_128x64.png");

Png.EncodeToFile(decodedPng, "test_64x64.png");


class ConsoleLogger : ILogger
{
    public void Debug(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }

    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void Warning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }

    public void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
} 