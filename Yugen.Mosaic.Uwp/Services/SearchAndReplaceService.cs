using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Toolkit.Standard.Mvvm.DependencyInjection;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public abstract class SearchAndReplaceService : ISearchAndReplaceService
    {
        protected readonly Rgba32[,] _avgsMaster;
        protected readonly Image<Rgba32> _outputImage;
        protected readonly List<Tile> _tileImageList;
        protected readonly Size _tileSize;
        protected readonly int _tX;
        protected readonly int _tY;
        protected readonly IProgressService _progressService;

        public SearchAndReplaceService(Image<Rgba32> outputImage, Size tileSize, int tX, int tY, List<Tile> tileImageList, 
            Rgba32[,] avgsMaster)
        {
            _outputImage = outputImage;
            _tileSize = tileSize;
            _tX = tX;
            _tY = tY;
            _tileImageList = tileImageList;
            _avgsMaster = avgsMaster;

            _progressService = Ioc.Default.GetService<IProgressService>();
        }

        public virtual void SearchAndReplace() { }

        // TODO: c.DrawImage crash (System.NullReferenceException)
        // with the current SixLabors.ImageSharp.Drawing preview version
        //internal void ApplyTileFound(int x, int y, Image<Rgba32> source)
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

        internal void ApplyTileFound(int x, int y, Image<Rgba32> source)
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