using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public abstract class SearchAndReplaceService : ISearchAndReplaceService
    {
        internal readonly Image<Rgba32> _outputImage;
        internal readonly Size _tileSize;
        internal readonly int _tX;
        internal readonly int _tY;
        internal readonly List<Tile> _tileImageList;
        internal readonly Rgba32[,] _avgsMaster;

        public SearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, List<Tile> tileImageList, Rgba32[,] avgsMaster)
        {
            _outputImage = outputImage;
            _tileSize = tileSize;
            _tX = tX;
            _tY = tY;
            _tileImageList = tileImageList;
            _avgsMaster = avgsMaster;
        }

        public virtual void SearchAndReplace() { }

        // TODO: check this out
        internal void ApplyTileFoundProcessor(int x, int y, Image<Rgba32> source)
        {
            _outputImage.Mutate(c =>
            {
                var point = new Point(x * source.Width, y * source.Height);
                try
                {
                    c.DrawImage(source, point, 1);
                }
                catch { }
            });
        }
    }
}
