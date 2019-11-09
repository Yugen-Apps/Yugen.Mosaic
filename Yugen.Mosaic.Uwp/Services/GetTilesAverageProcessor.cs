using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Threading.Tasks;

namespace Yugen.Mosaic.Uwp.Services
{
    public sealed class GetTilesAverageProcessor : IImageProcessor
    {
        private int _tX;
        private int _tY;
        private Size _tileSize;

        public Color[,] AvgsMaster;

        public GetTilesAverageProcessor(int tX, int tY, Size tileSize, Color[,] avgsMaster)
        {
            _tX = tX;
            _tY = tY;
            _tileSize = tileSize;

            AvgsMaster = avgsMaster;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new GetTilesAverageProcessor<TPixel>(this, source, sourceRectangle, _tX, _tY, _tileSize, AvgsMaster);
        }
    }

    public class GetTilesAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private int _tX;
        private int _tY;
        private Size _tileSize;

        public Color[,] AvgsMaster;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetTilesAverageProcessor(GetTilesAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle, int tX, int tY, Size tileSize, Color[,] avgsMaster)
        {
            Source = source;

            _tX = tX;
            _tY = tY;
            _tileSize = tileSize;

            AvgsMaster = avgsMaster;
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

                    var getTileAverageProcessor = new GetTileAverageProcessor(x * _tileSize.Width, y * _tileSize.Width, _tileSize.Width, _tileSize.Height, AvgsMaster, x, y);
                    Source.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));
                    //AvgsMaster[x, y] = getTileAverageProcessor.Color;
                }

            });
        }


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
