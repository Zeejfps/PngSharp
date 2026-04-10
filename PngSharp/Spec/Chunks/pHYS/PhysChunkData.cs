namespace PngSharp.Spec.Chunks.pHYS;

public readonly record struct PhysChunkData
{
    private const double MetersPerInch = 0.0254;

    public uint XAxisPPU { get; init; }
    public uint YAxisPPU { get; init; }
    public UnitSpecifier UnitSpecifier { get; init; }

    public double GetDpiX()
    {
        if (UnitSpecifier != UnitSpecifier.Meter)
            throw new InvalidOperationException("DPI conversion requires UnitSpecifier.Meter.");
        return XAxisPPU * MetersPerInch;
    }

    public double GetDpiY()
    {
        if (UnitSpecifier != UnitSpecifier.Meter)
            throw new InvalidOperationException("DPI conversion requires UnitSpecifier.Meter.");
        return YAxisPPU * MetersPerInch;
    }

    public static PhysChunkData FromDpi(double dpiX, double dpiY)
    {
        return new PhysChunkData
        {
            XAxisPPU = (uint)Math.Round(dpiX / MetersPerInch),
            YAxisPPU = (uint)Math.Round(dpiY / MetersPerInch),
            UnitSpecifier = UnitSpecifier.Meter,
        };
    }
}
