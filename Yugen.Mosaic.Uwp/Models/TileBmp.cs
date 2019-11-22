using Windows.UI.Xaml.Media.Imaging;

namespace Yugen.Mosaic.Uwp.Models
{
    public class TileBmp
    {
        public string Name { get; set; }
        public BitmapImage Image { get; set; }

        public TileBmp(string name, BitmapImage image)
        {
            Name = name;
            Image = image;
        }
    }
}
