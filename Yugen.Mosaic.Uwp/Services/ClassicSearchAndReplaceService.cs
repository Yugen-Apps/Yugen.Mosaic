using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class ClassicSearchAndReplaceService : SearchAndReplaceService
    {
        public ClassicSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, 
            int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster) 
                : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        public override void SearchAndReplace()
        {
            ProgressHelper.ResetProgress();

            int max = _tX * _tY;

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                int difference = 1000;
                List<TileFound> tileFoundList = new List<TileFound>();

                // Search for a tile with a similar color
                foreach (var tile in _tileImageList)
                {
                    var newDifference = ColorHelper.GetDifference(_avgsMaster[x, y], tile.AverageColor);
                    if (newDifference <= (difference + 5))
                    {
                        tileFoundList.Add(new TileFound(tile, newDifference));
                        difference = newDifference;
                    }
                }

                // Choose a random tile from tileFoundList with a threshold +/- 5 the best match
                var threshold = tileFoundList.Min(t1 => t1.Difference) + 5;
                var r = new Random();
                var tileFound = tileFoundList.Where(t2 => t2.Difference <= threshold)
                    .OrderBy(a => r.Next())
                        .First().Tile;

                // Apply found tile to section
                ApplyTileFound(x, y, tileFound.ResizedImage);

                ProgressHelper.IncrementProgress(66, 34, max);
            });
        }
    }
}