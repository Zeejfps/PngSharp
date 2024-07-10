# WIP

```C#
// Entry Point 
var pngApi = new PngApi(); // You can also pass an implementation of the ILogger interface if logging infromation is required

// To decode a PNG from a file
var decodedPng = pngApi.DecodeFromFile("Assets/sprite_atlas_128x64.png");

// To encode a PNG to a file
pngApi.EncodeToFile(decodedPng, "test_64x64.png");
```

