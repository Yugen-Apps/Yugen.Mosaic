using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public class ClassicSearchAndReplaceService : SearchAndReplaceService
    {
        public ClassicSearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster) : base(outputImage, tileSize, tX, tY, tileImageList, avgsMaster)
        {
        }

        public override void SearchAndReplace()
        {
            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                int index = 0;
                int difference = 100;
                Tile tileFound = _tileImageList[0];

                // Search for a tile with a similar color
                foreach (var tile in _tileImageList)
                {
                    var newDifference = ColorHelper.GetDifference(_avgsMaster[x, y], _tileImageList[index].AverageColor);
                    if (newDifference < difference)
                    {
                        tileFound = _tileImageList[index];
                        difference = newDifference;
                    }
                    index++;
                }

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                ApplyTileFoundProcessor(x, y, tileFound.ResizedImage);

                //_progress++;
            });
        }
    }
}
