using Windows.UI.Xaml.Media;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class ColorHelper
    {
        public static System.Drawing.Color Convert(Windows.UI.Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        public static Windows.UI.Color Convert(System.Drawing.Color color) => Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        
        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte r = (byte)System.Convert.ToUInt32(hex.Substring(0, 2), 16);
            byte g = (byte)System.Convert.ToUInt32(hex.Substring(2, 2), 16);
            byte b = (byte)System.Convert.ToUInt32(hex.Substring(4, 2), 16);
            byte a = (byte)System.Convert.ToUInt32(hex.Substring(6, 2), 16);
            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }
    }
}
