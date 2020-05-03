using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;
using System.Threading.Tasks;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class AdjustHueProcessor : IImageProcessor
    {
        public Image<Rgba32> InputImage { get; }
        public Rgba32 AverageColor { get; }

        public AdjustHueProcessor(Image<Rgba32> inputImage, Rgba32 averageColor)
        {
            InputImage = inputImage;
            AverageColor = averageColor;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new AdjustHueProcessor<TPixel>(configuration, this, source, sourceRectangle);
        }
    }

    public class AdjustHueProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> _source;
        private readonly Image<Rgba32> _inputImage;
        private readonly Rgba32 _averageColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustHueProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="AdjustHueProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public AdjustHueProcessor(Configuration configuration, AdjustHueProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            _source = source;
            _inputImage = definition.InputImage;
            _averageColor = definition.AverageColor;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            //int width = Source.Width;
            //Image<TPixel> source = Source;

            Parallel.For(0, _inputImage.Height, h =>
            {
                var rowSpan = _inputImage.GetPixelRowSpan(h);

                for (int w = 0; w < _inputImage.Width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    int R = Math.Min(255, Math.Max(0, (pixel.R + _averageColor.R) / 2));
                    int G = Math.Min(255, Math.Max(0, (pixel.G + _averageColor.G) / 2));
                    int B = Math.Min(255, Math.Max(0, (pixel.B + _averageColor.B) / 2));

                    Color clAvg = new Rgba32(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));
                    
                    TPixel pixelColor = clAvg.ToPixel<TPixel>();
                    _source[w, h] = pixelColor;
                }
            });
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
