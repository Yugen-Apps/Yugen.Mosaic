using SixLabors.ImageSharp;

namespace Yugen.Mosaic.Uwp.Models
{
    public class NewTile
    {
        public Image Image;
        public Color Color;

        public NewTile(Image image, Color color)
        {
            Image = image;
            Color = color;
        }
    }
}
