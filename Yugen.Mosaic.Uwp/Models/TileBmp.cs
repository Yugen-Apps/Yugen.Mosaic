using Windows.UI.Xaml.Media.Imaging;

namespace Yugen.Mosaic.Uwp.Models
{
    public class TileBmp
    {
        public string Name { get; set; }
        public WriteableBitmap Image { get; set; }

        public TileBmp(string name, WriteableBitmap image)
        {
            Name = name;
            Image = image;
        }
    }
}
