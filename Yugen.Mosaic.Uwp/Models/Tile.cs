using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public Image<Rgba32> OriginalImage { get; set; }
        public string Name { get; set; }

        public Image<Rgba32> ResizedImage { get; set; }
        public Rgba32 AverageColor { get; set; }

        public Tile(Image<Rgba32> originalImage, string name)
        {
            OriginalImage = originalImage;
            Name = name;
        }
    }
}
