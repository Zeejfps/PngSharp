using PngSharp.Api;
using Xunit;

namespace PngSharp.Tests;

public class PngFileTests
{
    [Fact]
    public void DecodeFromFile_ReadsAReadOnlyFile()
    {
        // Images shipped inside an AppImage or a read-only install sit on a read-only file system;
        // decoding must not ask for write access to them.
        var path = Path.Combine(Path.GetTempPath(), $"pngsharp-readonly-{Guid.NewGuid():N}.png");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Assets", "test_2x2.png"), path);
        File.SetAttributes(path, FileAttributes.ReadOnly);
        try
        {
            var png = Png.DecodeFromFile(path);

            Assert.Equal(2u, png.Ihdr.Width);
            Assert.Equal(2u, png.Ihdr.Height);
        }
        finally
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
        }
    }
}
