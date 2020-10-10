using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceService
    {
        void Init(Rgba32[,] avgsMaster, Size outputSize, List<Tile> tileImageList, Size tileSize, int tX, int tY);

        Image<Rgba32> SearchAndReplace();
    }
}