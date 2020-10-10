using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceAsciiArtService
    {
        Image<Rgba32> SearchAndReplace(Image<Rgba32> masterImage, int ratio = 5);
    }
}