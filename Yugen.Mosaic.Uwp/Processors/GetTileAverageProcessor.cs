using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System;
using System.Threading.Tasks;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class GetTileAverageProcessor : IImageProcessor
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public YugenColor MyColor { get; } = new YugenColor();

        public GetTileAverageProcessor(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new GetTileAverageProcessor<TPixel>(this, source, sourceRectangle);
        }
    }

    public class GetTileAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _height;

        private readonly YugenColor _myColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetTileAverageProcessor(GetTileAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            Source = source;

            _x = definition.X;
            _y = definition.Y;
            _width = definition.Width;
            _height = definition.Height;

            _myColor = definition.MyColor;
        }


        /// <inheritdoc/>
        public void Apply()
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(_y, _y+_height, h =>
            {
                var rowSpan = Source.GetPixelRowSpan(h);

                for (int w = _x; w < _x + _width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            });

            aR /= _width * _height;
            aG /= _width * _height;
            aB /= _width * _height;

            _myColor.ClAvg = new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB), 255);
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
