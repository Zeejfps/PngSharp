using PngSharp;

var decodedPng = Png.DecodeFromFile("Assets/sprite_atlas_128x64.png");
Png.EncodeToFile(decodedPng, "test_64x64.png");

