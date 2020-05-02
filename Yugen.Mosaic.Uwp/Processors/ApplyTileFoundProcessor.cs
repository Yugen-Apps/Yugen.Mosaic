using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Threading.Tasks;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class ApplyTileFoundProcessor : IImageProcessor
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public Image<Rgba32> OutputImage { get; }

        public ApplyTileFoundProcessor(int x, int y, int width, int height, Image<Rgba32> outputImage)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            OutputImage = outputImage;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new ApplyTileFoundProcessor<TPixel>(configuration, this, source, sourceRectangle);
        }
    }

    public class ApplyTileFoundProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> _source;

        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _height;

        private readonly Image<Rgba32> _outputImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public ApplyTileFoundProcessor(Configuration configuration, ApplyTileFoundProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            _source = source;

            _x = definition.X;
            _y = definition.Y;
            _width = definition.Width;
            _height = definition.Height;

            _outputImage = definition.OutputImage;
        }
        
        /// <inheritdoc/>
        public void Execute()
        {
            Parallel.For(0, _height, h =>
            {
                var rowSpan = _source.GetPixelRowSpan(h);

                for (int w = 0; w < _width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    _outputImage[_x * _width + w, _y * _height + h] = pixel;
                }
            });
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
