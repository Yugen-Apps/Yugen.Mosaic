using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public abstract class SearchAndReplaceService : ISearchAndReplaceService
    {
        protected readonly IProgressService _progressService;

        protected Rgba32[,] _avgsMaster;
        protected Image<Rgba32> _outputImage;
        protected List<Tile> _tileImageList;
        protected Size _tileSize;
        protected int _tX;
        protected int _tY;

        public SearchAndReplaceService(IProgressService progressService)
        {
            _progressService = progressService;
        }

        public void Init(Rgba32[,] avgsMaster, Size outputSize,
            List<Tile> tileImageList, Size tileSize, int tX, int tY)
        {
            _avgsMaster = avgsMaster;
            _tileImageList = tileImageList;
            _tileSize = tileSize;
            _tX = tX;
            _tY = tY;

            _outputImage = new Image<Rgba32>(tileSize.Width * _tX, tileSize.Height * _tY);
        }

        public virtual Image<Rgba32> SearchAndReplace() => throw new NotImplementedException();

        // TODO: c.DrawImage crash (System.NullReferenceException)
        // with the current SixLabors.ImageSharp.Drawing beta version
        //protected void ApplyTileFound(int x, int y, Image<Rgba32> source)
        //{
        //    _outputImage.Mutate(c =>
        //    {
        //        var point = new Point(x * source.Width, y * source.Height);
        //        try
        //        {
        //            c.DrawImage(source, point, 1);
        //        }
        //        catch { }
        //    });
        //}

        protected void ApplyTileFound(int x, int y, Image<Rgba32> source)
        {
            Parallel.For(0, source.Height, h =>
            {
                Span<Rgba32> rowSpan = source.GetPixelRowSpan(h);

                for (var w = 0; w < source.Width; w++)
                {
                    var pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    _outputImage[x * source.Width + w, y * source.Height + h] = pixel;
                }
            });
        }
    }
}