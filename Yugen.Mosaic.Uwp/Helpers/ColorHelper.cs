namespace Yugen.Mosaic.Uwp
{
    public static class ColorHelper
    {
        public static System.Drawing.Color Convert(Windows.UI.Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        public static Windows.UI.Color Convert(System.Drawing.Color color) => Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
