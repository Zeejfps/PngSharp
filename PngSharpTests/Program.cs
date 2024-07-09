using PngSharp.Api;

var pngApi = new PngApi();
var decodedPng = pngApi.DecodeFromFile("Assets/sprite_atlas_128x64.png");
pngApi.EncodeToFile(decodedPng, "test_64x64.png");