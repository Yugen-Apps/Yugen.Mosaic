using Windows.UI.Xaml.Media.Imaging;
using Color = System.Drawing.Color;

namespace Yugen.Mosaic.Uwp
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
