# PngSharp

A fast, low-allocation, pure C# PNG encoder/decoder with zero native dependencies.

## Features

- All color types and bit depths (1, 2, 4, 8, 16) per the PNG spec
- All 5 adaptive filter types with per-scanline selection
- Chunks: IHDR, PLTE, IDAT, IEND, tRNS, sRGB, gAMA, pHYs
- CRC-32 validation on all chunks
- Stackalloc and span-based paths to minimize heap allocations

## Decoding

```C#
// Decode from a file
var png = Png.DecodeFromFile("image.png");

// Decode from a byte array
var png = Png.DecodeFromByteArray(bytes);

// Decode from a stream
var png = Png.DecodeFromStream(stream);

// Access image data
var ihdr = png.Ihdr;
Console.WriteLine($"{ihdr.Width}x{ihdr.Height}, {ihdr.GetBytesPerPixel()} bytes per pixel");
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

## Builder

For full control over the PNG structure, use the builder:

```C#
var png = Png.Builder()
    .WithIhdr(new IhdrChunkData
    {
        Width = 256,
        Height = 256,
        BitDepth = 8,
        ColorType = ColorType.TrueColorWithAlpha,
        CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
        FilterMethod = FilterMethod.AdaptiveFiltering,
        InterlaceMethod = InterlaceMethod.None,
    })
    .WithPixelData(pixelData)
    .WithSrgb(new SrgbChunkData { RenderingIntent = RenderingIntent.Perceptual })
    .WithGama(new GammaChunkData { Value = 45455 })
    .WithPhys(new PhysChunkData { XAxisPPU = 3780, YAxisPPU = 3780, UnitSpecifier = UnitSpecifier.Meter })
    .Build();

Png.EncodeToFile(png, "output.png");
```

### Indexed color with palette and transparency

```C#
var png = Png.Builder()
    .WithIhdr(new IhdrChunkData
    {
        Width = 16,
        Height = 16,
        BitDepth = 8,
        ColorType = ColorType.IndexedColor,
        CompressionMethod = CompressionMethod.DeflateWithSlidingWindow,
        FilterMethod = FilterMethod.AdaptiveFiltering,
        InterlaceMethod = InterlaceMethod.None,
    })
    .WithPlte(new PlteChunkData { Entries = [255, 0, 0, 0, 255, 0, 0, 0, 255] }) // red, green, blue
    .WithTrns(new TrnsChunkData { Data = [255, 128, 0] }) // fully opaque, semi-transparent, fully transparent
    .WithPixelData(pixelIndices)
    .Build();
```

## Configuration

```C#
// Optional: specify a logger to see debug information
Png.Logger = new ConsoleLogger();

// Optional: specify a custom file system
Png.FileSystem = new CustomFileSystem();
```

