using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class ColorHelper
    {
        public static Rgba32 GetAverageColor(Image<Rgba32> source, int x, int y, Size tileSize) => GetAverageColor(source, x * tileSize.Width, y * tileSize.Height,
            tileSize.Width, tileSize.Height, x * tileSize.Width + tileSize.Width, y * tileSize.Height + tileSize.Height);

        public static Rgba32 GetAverageColor(Image<Rgba32> source) => GetAverageColor(source, 0, 0, source.Width, source.Height, source.Width, source.Height);

        private static Rgba32 GetAverageColor(Image<Rgba32> source, int startX, int startY, int width, int height, int endX, int endY)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(startY, endY, h =>
            {
                Span<Rgba32> rowSpan = source.GetPixelRowSpan(h);

                for (var w = startX; w < endX; w++)
                {
                    var pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            });

            aR /= width * height;
            aG /= width * height;
            aB /= width * height;

            return new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }


        public static int GetDifference(Rgba32 source, Rgba32 target)
        {
            var dR = Math.Abs(source.R - target.R);
            var dG = Math.Abs(source.G - target.G);
            var dB = Math.Abs(source.B - target.B);
            var diff = Math.Max(dR, dG);
            return Math.Max(diff, dB);
        }
    }
}
