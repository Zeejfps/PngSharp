using PngSharp.Api;

Png.Logger = new ConsoleLogger();

// Decode an existing PNG and re-encode it
var decodedPng = Png.DecodeFromFile("Assets/sprite_atlas_128x64.png");
Png.EncodeToFile(decodedPng, "test_64x64.png");

// Create a 2x2 red square (RGBA)
var redPixels = new byte[]
{
    255, 0, 0, 255,  255, 0, 0, 255,
    255, 0, 0, 255,  255, 0, 0, 255,
};
var redSquare = Png.CreateRgba(2, 2, redPixels);
Png.EncodeToFile(redSquare, "red_square_rgba.png");

// Create a 2x2 blue square (RGB, no alpha)
var bluePixels = new byte[]
{
    0, 0, 255,  0, 0, 255,
    0, 0, 255,  0, 0, 255,
};
var blueSquare = Png.CreateRgb(2, 2, bluePixels);
Png.EncodeToFile(blueSquare, "blue_square_rgb.png");

// Create a 2x2 grayscale gradient
var grayPixels = new byte[] { 0, 85, 170, 255 };
var grayGradient = Png.CreateGrayscale(2, 2, grayPixels);
Png.EncodeToFile(grayGradient, "gray_gradient.png");

// Create a 2x2 grayscale with alpha (checkerboard transparency)
var grayAlphaPixels = new byte[]
{
    255, 255,  128, 0,
    128, 0,    255, 255,
};
var grayAlpha = Png.CreateGrayscaleWithAlpha(2, 2, grayAlphaPixels);
Png.EncodeToFile(grayAlpha, "gray_alpha_checkerboard.png");


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