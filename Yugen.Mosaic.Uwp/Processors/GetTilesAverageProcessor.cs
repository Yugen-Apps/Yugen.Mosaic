using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System;
using System.Threading.Tasks;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class GetTilesAverageProcessor : IImageProcessor
    {
        public int TX { get; }
        public int TY { get; }
        public Size TileSize { get; }

        public Color[,] AvgsMaster { get; }

        public GetTilesAverageProcessor(int tX, int tY, Size tileSize, Color[,] avgsMaster)
        {
            TX = tX;
            TY = tY;
            TileSize = tileSize;

            AvgsMaster = avgsMaster;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new GetTilesAverageProcessor<TPixel>(this, source, sourceRectangle);
        }
    }

    public class GetTilesAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private readonly int _tX;
        private readonly int _tY;
        private Size _tileSize;

        private Color[,] _avgsMaster;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetTilesAverageProcessor(GetTilesAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            Source = source;

            _tX = definition.TX;
            _tY = definition.TY;
            _tileSize = definition.TileSize;

            _avgsMaster = definition.AvgsMaster;
        }

        /// <inheritdoc/>
        public void Apply()
        {
            Parallel.For(0, _tY, y =>
            {
                var rowSpan = Source.GetPixelRowSpan(y);

                for (int x = 0; x < _tX; x++)
                {
                    _avgsMaster[x, y] = GetTileAverage(Source, x * _tileSize.Width, y * _tileSize.Height, _tileSize.Width, _tileSize.Height); ;
                }
            });
        }

        private Color GetTileAverage(Image<TPixel> source, int x, int y, int width, int height)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(y, y + height, h =>
            {
                var rowSpan = Source.GetPixelRowSpan(h);

                for (int w = x; w < x + width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            });

            aR /= width * height;
            aG /= width * height;
            aB /= width * height;

            return Color.FromRgb(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
