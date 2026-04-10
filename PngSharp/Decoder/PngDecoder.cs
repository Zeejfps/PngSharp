using PngSharp.Api;
using PngSharp.Decoder.States;
using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.tIME;
using PngSharp.Spec.Chunks.tRNS;

namespace PngSharp.Decoder;

internal sealed class PngDecoder : IDisposable, IAsyncDisposable
{
    public PngReader Reader { get; }
    public IDecoderState State { get; set; }
    public IhdrChunkData IhdrChunkData { get; set; }
    public Stream CompressedPixelDataStream { get; }
    public Stream PixelDataStream { get; }
    public PlteChunkData? Plte { get; set; }
    public TrnsChunkData? Trns { get; set; }
    public SrgbChunkData? Srgb { get; set; }
    public GammaChunkData? Gama { get; set; }
    public PhysChunkData? Phys { get; set; }
    public ChrmChunkData? Chrm { get; set; }
    public TimeChunkData? Time { get; set; }
    public BkgdChunkData? Bkgd { get; set; }
    public List<TextChunk> TxtChunks { get; } = [];
    public List<ZTextChunk> ZTxtChunks { get; } = [];
    public List<ITextChunk> ITxtChunks { get; } = [];

    
    // States
    public IDecoderState ReadIhdrChunkState { get; }
    public IDecoderState ReadChunkState { get; }
    public IDecoderState DoneState { get; }
    public IDecoderState DecodePixelDataState { get; }
    public IDecoderState DecodeAdam7State { get; }
    
    private bool IsDone => State == DoneState;

    public PngDecoder(PngReader pngReader, ILogger logger)
    {
        Reader = pngReader;
        State = new ReadSignatureState(this);
        
        ReadIhdrChunkState = new ReadIhdrChunkState(this, logger);
        ReadChunkState = new ReadChunkState(this, logger);
        DecodePixelDataState = new DecodePixelDataState(this);
        DecodeAdam7State = new DecodeAdam7PixelDataState(this);
        DoneState = new DoneState();
        
        CompressedPixelDataStream = new MemoryStream();
        PixelDataStream = new MemoryStream();
    }

    public void Decode()
    {
        while (!IsDone)
            State.Execute();
    }
    
    public void Dispose()
    {
        CompressedPixelDataStream.Dispose();
        PixelDataStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await CompressedPixelDataStream.DisposeAsync();
        await PixelDataStream.DisposeAsync();
    }
}