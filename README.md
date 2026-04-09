# PngSharp

A pure C# PNG encoder/decoder library.

## Decoding

```C#
// Decode from a file
var png = Png.DecodeFromFile("image.png");

// Decode from a byte array
var png = Png.DecodeFromByteArray(bytes);

// Decode from a stream
var png = Png.DecodeFromStream(stream);

// Access image data
Console.WriteLine($"{png.Width}x{png.Height}, {png.BytesPerPixel} bytes per pixel");
byte[] pixels = png.PixelData;
```

## Encoding

```C#
// Encode to a file
Png.EncodeToFile(png, "output.png");

// Encode to a stream
Png.EncodeToStream(png, stream);
```

## Creating images from raw pixel data

```C#
// RGBA (4 bytes per pixel)
var rgba = Png.CreateRgba(width, height, pixelData);

// RGB (3 bytes per pixel)
var rgb = Png.CreateRgb(width, height, pixelData);

// Grayscale (1 byte per pixel)
var gray = Png.CreateGrayscale(width, height, pixelData);

// Grayscale with alpha (2 bytes per pixel)
var grayAlpha = Png.CreateGrayscaleWithAlpha(width, height, pixelData);

// Then encode
Png.EncodeToFile(rgba, "output.png");
```

## Configuration

```C#
// Optional: specify a logger to see debug information
Png.Logger = new ConsoleLogger();

// Optional: specify a custom file system
Png.FileSystem = new CustomFileSystem();
```

