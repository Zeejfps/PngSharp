using PngSharp.Common;
using PngSharp.Decoder.States;

namespace PngSharp.Decoder;

internal sealed class PngDecoder : IDisposable, IAsyncDisposable
{
    public PngReader Reader { get; }
    public IDecoderState State { get; set; }
    
    public IDecoderState ReadIhdrChunkState { get; }
    public IDecoderState ReadChunkState { get; }
    
    public PngSpec.IhdrChunkData IhdrChunkData { get; set; }
    public Stream CompressedPixelDataStream { get; }
    public Stream PixelDataStream { get; }

    public bool IsDone => State == DoneState;
    
    public IDecoderState DoneState { get; }
    public int BytesPerPixel => IhdrChunkData.GetBytesPerPixel();

    public PngDecoder(Stream stream)
    {
        Reader = new PngReader(stream);
        State = new ReadSignatureState(this);
        
        ReadIhdrChunkState = new ReadIhdrChunkState(this);
        ReadChunkState = new ReadChunkState(this);
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

    public PixelFormat GetPixelFormat()
    {
        return IhdrChunkData.ColorType switch
        {
            PngSpec.ColorType.Grayscale => PixelFormat.Grayscale,
            PngSpec.ColorType.TrueColor => PixelFormat.RGB,
            PngSpec.ColorType.IndexedColor =>
                // TODO: Fix
                // NOTE(Zee): Is this suppose to be RGBA?
                PixelFormat.RGBA,
            PngSpec.ColorType.GrayscaleWithAlpha => PixelFormat.GrayscaleWithAlpha,
            PngSpec.ColorType.TrueColorWithAlpha => PixelFormat.RGBA,
            _ => throw new ArgumentOutOfRangeException(nameof(IhdrChunkData))
        };
    }
}