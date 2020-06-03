using Windows.UI.Xaml.Media.Imaging;

namespace Yugen.Mosaic.Uwp.Models
{
    public class TileBmp
    {
        public TileBmp(string name, BitmapImage image)
        {
            Name = name;
            Image = image;
        }

        public BitmapImage Image { get; set; }
        public string Name { get; set; }
    }
}