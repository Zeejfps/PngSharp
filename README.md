# PngSharp

[![NuGet](https://img.shields.io/nuget/v/PngSharp)](https://www.nuget.org/packages/PngSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PngSharp)](https://www.nuget.org/packages/PngSharp)
[![CI](https://github.com/Zeejfps/PngSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/Zeejfps/PngSharp/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Zeejfps/PngSharp/blob/main/LICENSE)

A fast, low-allocation, pure C# PNG encoder/decoder with zero native dependencies.

## Features

- All color types and bit depths (1, 2, 4, 8, 16) per the PNG spec
- Adam7 interlacing (encode and decode)
- All 5 adaptive filter types with per-scanline selection
- Chunks: IHDR, PLTE, IDAT, IEND, tRNS, sRGB, gAMA, pHYs, cHRM, tIME, bKGD, sBIT, iCCP, eXIf, tEXt, zTXt, iTXt
- Chunk ordering validation per the PNG spec
- Multi-IDAT chunk encoding for large images
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

// Helper extension methods
PngSize size = png.GetDimensions();       // (Width, Height) tuple
long bytes = png.GetMemorySize();         // total pixel data size in bytes
bool alpha = png.HasAlphaChannel();       // true for GrayscaleWithAlpha / TrueColorWithAlpha
bool palette = png.HasPalette();          // true for IndexedColor
bool gray = png.IsGrayscale();            // true for Grayscale / GrayscaleWithAlpha
```

## Encoding

```C#
// Encode to a file
Png.EncodeToFile(png, "output.png");

// Encode to a byte array
byte[] bytes = Png.EncodeToByteArray(png);

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
    .WithGama(GammaChunkData.FromDouble(0.45455)) // or set raw: new GammaChunkData { Value = 45455 }
    .WithPhys(PhysChunkData.FromDpi(96, 96))      // or set raw PPU values
    .Build();

Png.EncodeToFile(png, "output.png");
```

### Adam7 interlaced encoding

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
        InterlaceMethod = InterlaceMethod.Adam7,
    })
    .WithPixelData(pixelData)
    .Build();

Png.EncodeToFile(png, "interlaced.png");
```

Interlaced images are automatically deinterlaced when decoded — `png.PixelData` always contains the final, non-interlaced pixel data.

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

### Text metadata

```C#
var png = Png.Builder()
    .WithIhdr(ihdr)
    .WithPixelData(pixelData)
    // Uncompressed Latin-1 text (tEXt)
    .WithTxtChunk(new TextChunk { Keyword = "Author", Text = "Jane Doe" })
    // Compressed Latin-1 text (zTXt)
    .WithZTxtChunk(ZTextChunk.Create(new ZTextContent { Keyword = "Comment", Text = "A long description..." }))
    // International UTF-8 text (iTXt)
    .WithITxtChunk(ITextChunk.Create(new ITextContent
    {
        Keyword = "Title",
        Text = "タイトル",
        LanguageTag = "ja",
        TranslatedKeyword = "タイトル",
    }))
    .Build();
```

### Significant bits

```C#
var png = Png.Builder()
    .WithIhdr(ihdr)
    .WithPixelData(pixelData)
    // sBIT: original significant bits per channel (e.g. 5-6-5 RGB source)
    .WithSbit(new SbitChunkData { Data = [5, 6, 5] }) // length depends on color type
    .Build();
```

### ICC color profile

```C#
// Embed an ICC profile (mutually exclusive with sRGB)
var content = new IccpChunkContent { ProfileName = "sRGB IEC61966-2.1", RawProfile = iccBytes };
var png = Png.Builder()
    .WithIhdr(ihdr)
    .WithPixelData(pixelData)
    .WithIccp(IccpChunkData.Encode(content))
    .Build();

// Read it back
var profile = png.Iccp!.Value.Decode();
Console.WriteLine($"Profile: {profile.ProfileName}, {profile.RawProfile.Length} bytes");
```

### EXIF metadata

```C#
// Embed raw EXIF data (must start with "MM" or "II" byte order mark)
var png = Png.Builder()
    .WithIhdr(ihdr)
    .WithPixelData(pixelData)
    .WithExif(new ExifChunkData { Data = exifBytes })
    .Build();
```

### Chromaticities, background color, and modification time

```C#
var png = Png.Builder()
    .WithIhdr(ihdr)
    .WithPixelData(pixelData)
    .WithChrm(new ChrmChunkData
    {
        WhitePointX = 31270, WhitePointY = 32900,
        RedX = 64000, RedY = 33000,
        GreenX = 30000, GreenY = 60000,
        BlueX = 15000, BlueY = 6000,
    })
    .WithBkgd(new BkgdChunkData { Data = [0, 0, 0, 0, 0, 0] }) // black background (RGB, 2 bytes each)
    .WithTime(TimeChunkData.FromDateTimeOffset(DateTimeOffset.UtcNow))
    .Build();
```

### Reading text chunks

```C#
var png = Png.DecodeFromFile("image.png");

foreach (var txt in png.TxtChunks)
    Console.WriteLine($"{txt.Keyword}: {txt.Text}");

foreach (var ztxt in png.ZTxtChunks)
{
    var content = ztxt.DecodeContent();
    Console.WriteLine($"{content.Keyword}: {content.Text}");
}

foreach (var itxt in png.ITxtChunks)
{
    var content = itxt.DecodeContent();
    Console.WriteLine($"{content.Keyword} [{content.LanguageTag}]: {content.Text}");
}
```

## Chunk Data Helpers

Several chunk data structs provide convenience methods for common conversions:

```C#
// Physical dimensions (pHYs) — DPI conversion
var phys = PhysChunkData.FromDpi(300, 300);
double dpiX = phys.GetDpiX(); // 300.0
double dpiY = phys.GetDpiY(); // 300.0

// Gamma (gAMA) — double conversion
var gama = GammaChunkData.FromDouble(0.45455);
double gamma = gama.ToDouble(); // 0.45455

// Modification time (tIME) — DateTimeOffset conversion
var time = TimeChunkData.FromDateTimeOffset(DateTimeOffset.UtcNow);
DateTimeOffset dt = time.ToDateTimeOffset();

// ICC profile (iCCP) — compress/decompress
var iccp = IccpChunkData.Encode(new IccpChunkContent { ProfileName = "sRGB", RawProfile = rawBytes });
IccpChunkContent content = iccp.Decode();

// Chromaticities (cHRM) — scaled double conversion
var chrm = ChrmChunkData.Create(new ChrmChunkContent { WhitePointX = 0.3127, WhitePointY = 0.3290, ... });
ChrmChunkContent values = chrm.DecodeContent();

// IHDR helpers
int bpp = ihdr.GetBytesPerPixel();
int bitsPerPixel = ihdr.GetBitsPerPixel();
int scanlineBytes = ihdr.GetScanlineByteWidth();
```

## Gamma Correction

Extension methods for applying gamma correction to decoded pixel data. All methods return a new `byte[]` — the original `PixelData` is never mutated. Alpha channels are never gamma corrected per the PNG spec.

```C#
var png = Png.DecodeFromFile("image.png");

// Query the effective file gamma (sRGB > gAMA precedence)
double? gamma = png.GetFileGamma(); // ~0.45455 for sRGB, or the gAMA value, or null

// Apply PNG spec gamma correction: output = input ^ (1 / (fileGamma * displayGamma))
byte[] corrected = png.ApplyGammaCorrection();          // default displayGamma = 2.2
byte[] corrected = png.ApplyGammaCorrection(1.8);       // custom display gamma

// Convert to linear light (gamma 1.0)
// Uses precise sRGB piecewise transfer when sRGB chunk is present
byte[] linear = png.ToLinear();

// Convert to sRGB encoding
// If already sRGB-tagged, returns a clone; otherwise linearizes then applies sRGB transfer
byte[] srgb = png.ToSrgb();

// Get a gamma-corrected palette (for indexed color images)
PlteChunkData? palette = png.GetGammaCorrectedPalette();
```

For indexed color images, `ApplyGammaCorrection`, `ToLinear`, and `ToSrgb` expand the pixel data to RGB (3 bytes per pixel) with corrected palette values. Use `GetGammaCorrectedPalette` if you want to keep the indexed format.

## Configuration

```C#
// Optional: specify a logger to see debug information
Png.Logger = new ConsoleLogger();

// Optional: specify a custom file system
Png.FileSystem = new CustomFileSystem();
```

