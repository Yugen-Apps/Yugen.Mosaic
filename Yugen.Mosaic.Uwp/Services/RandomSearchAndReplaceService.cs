using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.Services
{
    public class RandomSearchAndReplaceService : SearchAndReplaceService
    {
        public RandomSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, 
            List<Tile> tileImageList, Rgba32[,] avgsMaster) 
                : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        // Don't adjust hue - keep searching for a tile close enough
        public override void SearchAndReplace()
        {
            var r = new Random();

            ProgressService.Instance.Reset();

            int max = _tX * _tY;

            Parallel.For(0, _tX * _tY, xy =>
            {
                var y = xy / _tX;
                var x = xy % _tX;

                // Reset searching variables
                var threshold = 0;
                var searchCounter = 0;
                Tile tileFound = null;

                // Search for a tile with a similar color
                while (tileFound == null)
                {
                    var index = r.Next(_tileImageList.Count);
                    var difference = ColorHelper.GetDifference(_avgsMaster[x, y], _tileImageList[index].AverageColor);
                    if (difference < threshold)
                    {
                        tileFound = _tileImageList[index];
                    }
                    else
                    {
                        searchCounter++;
                        if (searchCounter >= _tileImageList.Count)
                        {
                            threshold += 5;
                        }
                    }
                }

                // Apply found tile to section
                ApplyTileFound(x, y, tileFound.ResizedImage);

                ProgressService.Instance.IncrementProgress(max, 66, 100);
            });
        }
    }
}