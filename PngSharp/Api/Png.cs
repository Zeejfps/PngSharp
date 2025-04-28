namespace PngSharp.Api;

public static class Png
{
    public static PngApi Api { get; } = new(new NullLogger());
    
    private sealed class NullLogger : ILogger
    {
        public void Debug(string message) { }
        public void Info(string message) { }
        public void Warning(string message) { }
        public void Error(string message) { }
    }
}