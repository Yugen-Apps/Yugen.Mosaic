using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public sealed class GetTileAverageProcessor : IImageProcessor
    {
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        
        public Color[,] AvgsMaster;
        private int _avgX;
        private int _avgY;


        public GetTileAverageProcessor(int x, int y, int width, int height, Color[,] avgsMaster, int avgX, int avgY)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;

            AvgsMaster = avgsMaster;
            _avgX = avgX;
            _avgY = avgY;
        }


        public MyColor MyColor;

        public GetTileAverageProcessor(int x, int y, int width, int height, MyColor myColor)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;

            MyColor = myColor;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new GetTileAverageProcessor<TPixel>(this, source, sourceRectangle, _x, _y, _width, _height, AvgsMaster, _avgX, _avgY, MyColor);
        }
    }

    public class GetTileAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private int _x;
        private int _y;
        private int _width;
        private int _height;

        public Color[,] AvgsMaster;
        private int _avgX;
        private int _avgY;

        public MyColor MyColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetTileAverageProcessor(GetTileAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle, int x, int y, int width, int height, Color[,] avgsMaster, int avgX, int avgY, MyColor myColor)
        {
            Source = source;

            _x = x;
            _y = y;
            _width = width;
            _height = height;

            AvgsMaster = avgsMaster;
            _avgX = avgX;
            _avgY = avgY;

            MyColor = myColor;
        }


        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source; // Avoid capturing this

            long aR = 0;
            long aG = 0;
            long aB = 0;

            for (int h = _y; h < _y + _height; h++)
            {
                var rowSpan = source.GetPixelRowSpan(h);

                for (int w = _x; w < _x + _width; w++)
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

            if (AvgsMaster == null)
            {
                MyColor.R = aR;
                MyColor.G = aG;
                MyColor.B = aB;
            }
            else
            {
                AvgsMaster[_avgX, _avgY] = Color.FromRgb(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB)); ;
            }
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
