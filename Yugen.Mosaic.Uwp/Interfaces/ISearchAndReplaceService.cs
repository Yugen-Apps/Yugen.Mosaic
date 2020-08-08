using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceService
    {
        void Init(Rgba32[,] avgsMaster, Image<Rgba32> outputImage, List<Tile> tileImageList, Size tileSize, int tX, int tY);
        void SearchAndReplace();
    }
}