using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;
using System.Threading.Tasks;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class GetTilesAverageProcessor : IImageProcessor
    {
        public int TX { get; }
        public int TY { get; }
        public Size TileSize { get; }

        public Rgba32[,] AverageColors { get; }

        public GetTilesAverageProcessor(int tX, int tY, Size tileSize, Rgba32[,] avgsMaster)
        {
            TX = tX;
            TY = tY;
            TileSize = tileSize;

            AverageColors = avgsMaster;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new GetTilesAverageProcessor<TPixel>(configuration, this, source, sourceRectangle);
        }
    }

    public class GetTilesAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> _source;

        private readonly int _tX;
        private readonly int _tY;
        private Size _tileSize;

        private readonly Rgba32[,] _averageColors;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetTilesAverageProcessor(Configuration configuration, GetTilesAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            _source = source;

            _tX = definition.TX;
            _tY = definition.TY;
            _tileSize = definition.TileSize;

            _averageColors = definition.AverageColors;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            Parallel.For(0, _tY, y =>
            {
                var rowSpan = _source.GetPixelRowSpan(y);

                for (int x = 0; x < _tX; x++)
                {
                    _averageColors[x, y].FromRgba32(GetTileAverage(_source, x * _tileSize.Width, y * _tileSize.Height, _tileSize.Width, _tileSize.Height));
                }
            });
        }

        private Rgba32 GetTileAverage(Image<TPixel> source, int x, int y, int width, int height)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(y, y + height, h =>
            {
                var rowSpan = _source.GetPixelRowSpan(h);

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

            return new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
