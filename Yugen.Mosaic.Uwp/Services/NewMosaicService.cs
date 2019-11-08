using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class NewMosaicService
    {
        public List<Tile> TileBmpList { get; set; } = new List<Tile>();

        private int tX;
        private int tY;
        private Color[,] avgsMaster;

        private Size _tileSize;

        private readonly BenchmarkHelper benchmarkHelper = new BenchmarkHelper();

        internal void GenerateMosaic(Image resizedMasterImage, Size outputSize, List<WriteableBitmap> list, Size tileSize, bool isAdjustHue)
        {
            _tileSize = tileSize;
            tX = resizedMasterImage.Width / tileSize.Width;
            tY = resizedMasterImage.Height / tileSize.Height;
            avgsMaster = new Color[tX, tY];

            GetTilesAverage(resizedMasterImage);
        }

        private void GetTilesAverage(Image resizedMasterImage)
        {
            benchmarkHelper.Start();

            var imageProcessor = new MyImageProcessor(avgsMaster, _tileSize, tX, tY);
            resizedMasterImage.Mutate(c => c.ApplyProcessor(imageProcessor));

            benchmarkHelper.Stop("2");
        }
    }

    public sealed class MyImageProcessor : IImageProcessor
    {
        public Color[,] AvgsMaster;

        private Size _tileSize;
        private int _tX;
        private int _tY;

        public MyImageProcessor(Color[,] avgsMaster, Size tileSize, int tX, int tY)
        {
            AvgsMaster = avgsMaster;
            _tileSize = tileSize;
            _tX = tX;
            _tY = tY;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new MyImageProcessor<TPixel>(this, source, sourceRectangle, AvgsMaster, _tileSize, _tX, _tY);
        }
    }

    public class MyImageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        public Color[,] AvgsMaster;

        private Size _tileSize;
        private int _tX;
        private int _tY;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public MyImageProcessor(MyImageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle, Color[,] avgsMaster, Size tileSize, int tX, int tY)
        {
            Source = source;
            AvgsMaster = avgsMaster;
            _tileSize = tileSize;
            _tX = tX;
            _tY = tY;
        }

        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source; // Avoid capturing this

            Parallel.For(0, _tY, y =>
            {
                var rowSpan = source.GetPixelRowSpan(y);

                for (int x = 0; x < _tX; x++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[x].ToRgba32(ref pixel);

                    var imageProcessor2 = new MyImageProcessor2(AvgsMaster, _tileSize, x, y, _tileSize.Width, _tileSize.Height);
                    Source.Mutate(c => c.ApplyProcessor(imageProcessor2));
                }

            });
        }


        /// <inheritdoc/>
        public void Dispose() { }
    }

    public sealed class MyImageProcessor2 : IImageProcessor
    {
        public Color[,] AvgsMaster;

        private Size _tileSize;

        int _x;
        int _y;
        int _width;
        int _height;

        public MyImageProcessor2(Color[,] avgsMaster, Size tileSize, int x, int y, int width, int height)
        {
            AvgsMaster = avgsMaster;
            _tileSize = tileSize;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new MyImageProcessor2<TPixel>(this, source, sourceRectangle, AvgsMaster, _tileSize, _x, _y, _width, _height);
        }
    }

    public class MyImageProcessor2<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        public Color[,] AvgsMaster;

        private Size _tileSize;

        int _x;
        int _y;
        int _width;
        int _height;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public MyImageProcessor2(MyImageProcessor2 definition, Image<TPixel> source, Rectangle sourceRectangle, Color[,] avgsMaster, Size tileSize, int x, int y, int width, int height)
        {
            Source = source;
            AvgsMaster = avgsMaster;
            _tileSize = tileSize;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source; // Avoid capturing this

            long aR = 0;
            long aG = 0;
            long aB = 0;

            var x = _x * _width;
            var y = _y * _height;

            for (int h = y; h < y + _height; h++)
            {
                var rowSpan = source.GetPixelRowSpan(h);

                for (int w = x; w < x + _width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            }

            aR /= width * _height;
            aG /= width * _height;
            aB /= width * _height;

            AvgsMaster[_x, _y] = Color.FromRgb(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
