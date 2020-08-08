using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public class SearchAndReplaceAdjustHueService : SearchAndReplaceService
    {
        public SearchAndReplaceAdjustHueService(IProgressService progressService) : base(progressService) { }

        public override void SearchAndReplace()
        {
            var r = new Random();
            var seq = Enumerable.Range(0, _tX * _tY).Select(x => x % _tileImageList.Count);
            var tileShuffledList = seq.OrderBy(a => r.Next());

            _progressService.Reset();

            int max = _tX * _tY;

            Parallel.For(0, _tX * _tY, xy =>
            {
                var y = xy / _tX;
                var x = xy % _tX;

                // tile coordinates (Row * ColCount) + Column
                var tileXY = (y * _tX) + x;
                var index = tileShuffledList.ElementAt(tileXY);

                // Get tile from index
                Tile tileFound = _tileImageList[index];

                // Adjust the hue
                var adjustedImage = new Image<Rgba32>(tileFound.ResizedImage.Width, tileFound.ResizedImage.Height);
                AdjustHue(tileFound.ResizedImage, adjustedImage, _avgsMaster[x, y]);

                // Apply found tile to section
                ApplyTileFound(x, y, adjustedImage);

                _progressService.IncrementProgress(max, 66, 100);
            });
        }

        private void AdjustHue(Image<Rgba32> source, Image<Rgba32> output, Rgba32 averageColor)
        {
            output.Mutate(c =>
            {
                Parallel.For(0, source.Height, h =>
                {
                    var rowSpan = source.GetPixelRowSpan(h);

                    for (int w = 0; w < source.Width; w++)
                    {
                        Rgba32 pixel = new Rgba32();
                        rowSpan[w].ToRgba32(ref pixel);

                        int R = Math.Min(255, Math.Max(0, (pixel.R + averageColor.R) / 2));
                        int G = Math.Min(255, Math.Max(0, (pixel.G + averageColor.G) / 2));
                        int B = Math.Min(255, Math.Max(0, (pixel.B + averageColor.B) / 2));

                        Color clAvg = new Rgba32(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

                        Rgba32 pixelColor = clAvg.ToPixel<Rgba32>();
                        output[w, h] = pixelColor;
                    }
                });
            });
        }
    }
}