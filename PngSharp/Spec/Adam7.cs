namespace PngSharp.Spec;

internal static class Adam7
{
    public const int PassCount = 7;

    private static readonly int[] ColStart = [0, 4, 0, 2, 0, 1, 0];
    private static readonly int[] ColInc   = [8, 8, 4, 4, 2, 2, 1];
    private static readonly int[] RowStart = [0, 0, 4, 0, 2, 0, 1];
    private static readonly int[] RowInc   = [8, 8, 8, 4, 4, 2, 2];

    public static int GetPassWidth(int imageWidth, int pass)
    {
        if (imageWidth <= ColStart[pass])
            return 0;
        return (imageWidth - ColStart[pass] + ColInc[pass] - 1) / ColInc[pass];
    }

    public static int GetPassHeight(int imageHeight, int pass)
    {
        if (imageHeight <= RowStart[pass])
            return 0;
        return (imageHeight - RowStart[pass] + RowInc[pass] - 1) / RowInc[pass];
    }

    public static int GetPassScanlineByteWidth(int passWidth, int bitsPerPixel)
    {
        return (passWidth * bitsPerPixel + 7) / 8;
    }

    public static int GetColStart(int pass) => ColStart[pass];
    public static int GetColInc(int pass) => ColInc[pass];
    public static int GetRowStart(int pass) => RowStart[pass];
    public static int GetRowInc(int pass) => RowInc[pass];
}
