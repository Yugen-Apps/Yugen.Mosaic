using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class PlainColorSearchAndReplaceService : SearchAndReplaceService
    {
        public PlainColorSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, 
            int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster) 
                : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        // Use just mosic colored tiles
        public override void SearchAndReplace()
        {
            ProgressHelper.ResetProgress();

            int max = _tX * _tY;

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // Generate colored tile
                var adjustedImage = new Image<Rgba32>(_tileSize.Width, _tileSize.Height);
                var averageColor4 = _avgsMaster[x, y].ToVector4();

                adjustedImage.Mutate(c => c.ProcessPixelRowsAsVector4(row =>
                {
                    foreach (ref Vector4 pixel in row)
                    {
                        pixel = (pixel + averageColor4) / 2;
                    }
                }));

                // Apply found tile to section
                ApplyTileFound(x, y, adjustedImage);

                ProgressHelper.IncrementProgress(66, 34, max);
            });
        }
    }
}