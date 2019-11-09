using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public Image<Rgba32> Image;
        public Color Color;

        public Tile(Image<Rgba32> image, Color color)
        {
            Image = image;
            Color = color;
        }
    }
}
