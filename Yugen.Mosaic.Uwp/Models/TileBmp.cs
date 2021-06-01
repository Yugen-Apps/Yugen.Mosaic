using Windows.UI.Xaml.Media;

namespace Yugen.Mosaic.Uwp.Models
{
    public class TileBmp
    {
        public TileBmp(string name, ImageSource image)
        {
            Name = name;
            Image = image;
        }

        public ImageSource Image { get; set; }

        public string Name { get; set; }
    }
}