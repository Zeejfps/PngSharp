using PngSharp.Api;
using PngSharp.Decoder.States;
using PngSharp.Spec;
using PngSharp.Spec.Chunks.IHDR;

namespace PngSharp.Decoder;

internal sealed class PngDecoder : IDisposable, IAsyncDisposable
{
    public PngReader Reader { get; }
    public IDecoderState State { get; set; }
    public IhdrChunkData IhdrChunkData { get; set; }
    public Stream CompressedPixelDataStream { get; }
    public Stream PixelDataStream { get; }
    public int BytesPerPixel => IhdrChunkData.GetBytesPerPixel();
    
    // States
    public IDecoderState ReadIhdrChunkState { get; }
    public IDecoderState ReadChunkState { get; }
    public IDecoderState DoneState { get; }
    
    public DecodedPng DecodedPng { get; } = new();

    private bool IsDone => State == DoneState;

    public PngDecoder(PngReader pngReader, ILogger logger)
    {
        Reader = pngReader;
        State = new ReadSignatureState(this);
        
        ReadIhdrChunkState = new ReadIhdrChunkState(this, logger);
        ReadChunkState = new ReadChunkState(this, logger);
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