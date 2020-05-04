using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Models;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public class AdjustHueSearchAndReplaceService : SearchAndReplaceService
    {
        public AdjustHueSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster) : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        // Adjust hue - get the first (random) tile found and adjust its colours to suit the average
        public override void SearchAndReplace()
        {
            Random r = new Random();
            List<Tile> tileQueue = new List<Tile>();
            //int maxQueueLength = Math.Min(1000, Math.Max(0, _tileImageList.Count - 50));

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // (R * ColCount) + C
                int index = ((y * _tX) + x) % _tileImageList.Count;

                // Check if it's the same as the last (X)?
                //if (tileQueue.Count > 1)
                //{
                //    while (tileQueue.Contains(_tileImageList[index]))
                //    {
                //        index = r.Next(_tileImageList.Count);
                //    }
                //}

                // Add to the 'queue'
                Tile tileFound = _tileImageList[index];
                //if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0)
                //    tileQueue.RemoveAt(0);
                //tileQueue.Add(tileFound);

                // Adjust the hue
                Image<Rgba32> adjustedImage = new Image<Rgba32>(tileFound.ResizedImage.Width, tileFound.ResizedImage.Height);

                //var adjustHueProcessor = new AdjustHueProcessor(tileFound.ResizedImage, _avgsMaster[x, y]);
                //adjustedImage.Mutate(c => c.ApplyProcessor(adjustHueProcessor));

                AdjustHue(tileFound.ResizedImage, adjustedImage, _avgsMaster[x, y]);

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //adjustedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, adjustedImage);

                //_progress++;
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
