﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public class RandomSearchAndReplaceService : SearchAndReplaceService
    {
        public RandomSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster) : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        // Don't adjust hue - keep searching for a tile close enough
        public override void SearchAndReplace()
        {
            Random r = new Random();

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // Reset searching variables
                int threshold = 0;
                int searchCounter = 0;
                Tile tileFound = null;

                // Search for a tile with a similar color
                while (tileFound == null)
                {
                    int index = r.Next(_tileImageList.Count);
                    var difference = ColorHelper.GetDifference(_avgsMaster[x, y], _tileImageList[index].AverageColor);
                    if (difference < threshold)
                    {
                        tileFound = _tileImageList[index];
                    }
                    else
                    {
                        searchCounter++;
                        if (searchCounter >= _tileImageList.Count)
                            threshold += 5;
                    }
                }

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, tileFound.ResizedImage);

                //_progress++;
            });
        }
    }
}
