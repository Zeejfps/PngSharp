using PngSharp.Api;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.PLTE;

namespace PngSharp.Spec;

internal static class GammaUtils
{
    internal static void GuardBitDepth(IRawPng png)
    {
        if (png.Ihdr.BitDepth != 8)
            throw new InvalidOperationException(
                $"Gamma correction is only supported for 8-bit images. This image has {png.Ihdr.BitDepth}-bit depth.");
    }

    internal static byte[] BuildLut(Func<double, double> transfer)
    {
        var lut = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            var normalized = i / 255.0;
            var corrected = transfer(normalized);
            lut[i] = (byte)Math.Clamp(Math.Round(corrected * 255.0), 0, 255);
        }
        return lut;
    }

    internal static double SrgbToLinear(double c)
    {
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    internal static double LinearToSrgb(double c)
    {
        return c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
    }

    internal static PlteChunkData CorrectPalette(PlteChunkData plte, byte[] lut)
    {
        var entries = plte.Entries;
        var corrected = new byte[entries.Length];
        for (var i = 0; i < entries.Length; i++)
            corrected[i] = lut[entries[i]];
        return new PlteChunkData { Entries = corrected };
    }

    internal static byte[] ExpandIndexedToRgb(IRawPng png, byte[]? lut)
    {
        var palette = png.Plte!.Value.Entries;
        var pixelData = png.PixelData;
        var result = new byte[pixelData.Length * 3];
        for (var i = 0; i < pixelData.Length; i++)
        {
            var idx = pixelData[i] * 3;
            var outIdx = i * 3;
            if (lut != null)
            {
                result[outIdx] = lut[palette[idx]];
                result[outIdx + 1] = lut[palette[idx + 1]];
                result[outIdx + 2] = lut[palette[idx + 2]];
            }
            else
            {
                result[outIdx] = palette[idx];
                result[outIdx + 1] = palette[idx + 1];
                result[outIdx + 2] = palette[idx + 2];
            }
        }
        return result;
    }

    internal static byte[] ApplyLutToPixels(IRawPng png, byte[] lut)
    {
        if (png.Ihdr.ColorType == ColorType.IndexedColor)
            return ExpandIndexedToRgb(png, lut);

        var result = (byte[])png.PixelData.Clone();
        var bytesPerPixel = png.Ihdr.GetBytesPerPixel();

        var colorChannels = png.Ihdr.ColorType switch
        {
            ColorType.Grayscale => 1,
            ColorType.TrueColor => 3,
            ColorType.GrayscaleWithAlpha => 1,
            ColorType.TrueColorWithAlpha => 3,
            _ => throw new InvalidOperationException($"Unsupported color type: {png.Ihdr.ColorType}")
        };

        for (var i = 0; i < result.Length; i += bytesPerPixel)
        {
            for (var c = 0; c < colorChannels; c++)
                result[i + c] = lut[result[i + c]];
        }

        return result;
    }
}
