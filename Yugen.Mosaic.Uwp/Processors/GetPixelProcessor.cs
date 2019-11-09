using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class GetPixelProcessor : IImageProcessor
    {
        public int X { get; }
        public int Y { get; }

        public YugenColor MyColor { get; } = new YugenColor();

        public GetPixelProcessor(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new GetPixelProcessor<TPixel>(this, source, sourceRectangle);
        }
    }

    public class GetPixelProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private readonly int _x;
        private readonly int _y;

        private readonly YugenColor _myColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public GetPixelProcessor(GetPixelProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            Source = source;

            _x = definition.X;
            _y = definition.Y;

            _myColor = definition.MyColor;
        }


        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source; // Avoid capturing this

            Rgba32 pixel = new Rgba32();
            source[_x, _y].ToRgba32(ref pixel);

            _myColor.ClAvg = pixel;
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
