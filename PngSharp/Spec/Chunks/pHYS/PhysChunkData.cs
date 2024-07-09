namespace PngSharp.Spec.Chunks.pHYS;

public readonly struct PhysChunkData
{
    public uint XAxisPPU { get; init; }
    public uint YAxisPPU { get; init; }
    public UnitSpecifier UnitSpecifier { get; init; }

    public override string ToString()
    {
        return
            $"{nameof(XAxisPPU)}: {XAxisPPU}, {nameof(YAxisPPU)}: {YAxisPPU}, {nameof(UnitSpecifier)}: {UnitSpecifier}";
    }
}