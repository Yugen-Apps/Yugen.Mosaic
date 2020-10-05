using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceAsciiArtService
    {
        void Init(Image<Rgba32> resizedMasterImage, Image<Rgba32> outputImage);
        Image<Rgba32> SearchAndReplace(int ratio = 5);
    }
}