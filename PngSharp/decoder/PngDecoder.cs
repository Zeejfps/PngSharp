namespace PngSharp.PngSharp;

internal sealed class PngDecoder : IDisposable, IAsyncDisposable
{
    public PngReader Reader { get; }
    public IDecoderState State { get; set; }
    public IDecoderState ReadIhdrChunkState { get; }
    public IDecoderState ReadChunkState { get; }
    public PngSpec.IhdrChunkData IhdrChunkData { get; set; }
    
    public MemoryStream PixelDataStream { get; }

    public PngDecoder()
    {
        
        State = new ReadSignatureState(this);
        ReadIhdrChunkState = new ReadIhdrChunkState(this);
        ReadChunkState = new ReadChunkState(this);
        PixelDataStream = new MemoryStream();
    }

    public void Update()
    {
        State.Execute();
    }

    public void Dispose()
    {
        PixelDataStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await PixelDataStream.DisposeAsync();
    }
}