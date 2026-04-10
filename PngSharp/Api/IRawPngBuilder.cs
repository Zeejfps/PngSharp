using PngSharp.Spec.Chunks.IHDR;
using PngSharp.Spec.Chunks.pHYS;
using PngSharp.Spec.Chunks.PLTE;
using PngSharp.Spec.Chunks.sGAMA;
using PngSharp.Spec.Chunks.sRGB;
using PngSharp.Spec.Chunks.Text;
using PngSharp.Spec.Chunks.bKGD;
using PngSharp.Spec.Chunks.cHRM;
using PngSharp.Spec.Chunks.tIME;
using PngSharp.Spec.Chunks.tRNS;

namespace PngSharp.Api;

public interface IRawPngBuilder
{
    IRawPngBuilder WithIhdr(IhdrChunkData ihdr);
    IRawPngBuilder WithPixelData(byte[] pixels);
    IRawPngBuilder WithPlte(PlteChunkData plte);
    IRawPngBuilder WithTrns(TrnsChunkData trns);
    IRawPngBuilder WithSrgb(SrgbChunkData srgb);
    IRawPngBuilder WithGama(GammaChunkData gama);
    IRawPngBuilder WithPhys(PhysChunkData phys);
    IRawPngBuilder WithChrm(ChrmChunkData chrm);
    IRawPngBuilder WithTime(TimeChunkData time);
    IRawPngBuilder WithBkgd(BkgdChunkData bkgd);
    IRawPngBuilder WithTxtChunk(TextChunk textChunk);
    IRawPngBuilder WithZTxtChunk(ZTextChunk textChunk);
    IRawPngBuilder WithITxtChunk(ITextChunk textChunk);
    IRawPng Build();
}
