using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceAsciiArtService
    {
        string Text { get; }

        Image<Rgba32> SearchAndReplace(Image<Rgba32> masterImage, int ratio = 5);
    }
}