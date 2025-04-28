# WIP

```C#
// You can specify a logger if you want to see logging information
// Png.Logger = new ConsoleLogger(); 

// You can also specify a file system to use if needed
// Png.FileSystem = new CustomFileSystem();

// To decode a PNG from a file
var decodedPng = Png.DecodeFromFile("Assets/sprite_atlas_128x64.png");

// To encode a PNG to a file
Png.EncodeToFile(decodedPng, "test_64x64.png");
```

