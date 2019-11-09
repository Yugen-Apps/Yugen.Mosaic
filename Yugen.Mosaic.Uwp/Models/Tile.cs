using SixLabors.ImageSharp;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public Image Image;
        public Color Color;

        public Tile(Image image, Color color)
        {
            Image = image;
            Color = color;
        }
    }
}
