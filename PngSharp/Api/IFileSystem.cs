namespace PngSharp.Api;

public interface IFileSystem
{
    Stream CreateFile(string pathToFile);
    Stream OpenFile(string pathToFile);
}