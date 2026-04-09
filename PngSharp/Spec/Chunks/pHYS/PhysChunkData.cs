namespace PngSharp.Spec.Chunks.pHYS;

public readonly record struct PhysChunkData
{
    public uint XAxisPPU { get; init; }
    public uint YAxisPPU { get; init; }
    public UnitSpecifier UnitSpecifier { get; init; }
}
