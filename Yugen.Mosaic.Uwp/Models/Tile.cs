using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public WriteableBitmap bitmap;
        public Color color;

        public Tile(WriteableBitmap bSource, Color cSource)
        {
            bitmap = bSource;
            color = cSource;
        }
    }
}
