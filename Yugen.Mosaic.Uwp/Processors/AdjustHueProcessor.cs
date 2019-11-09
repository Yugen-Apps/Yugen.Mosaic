using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class AdjustHueProcessor : IImageProcessor
    {
        private Color _color;

        public YugenColor MyColor;

        public AdjustHueProcessor(Color color, YugenColor myColor)
        {
            _color = color;

            MyColor = myColor;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new AdjustHueProcessor<TPixel>(this, source, sourceRectangle, _color, MyColor);
        }
    }

    public class AdjustHueProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private Color _color;

        public YugenColor MyColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public AdjustHueProcessor(AdjustHueProcessor definition, Image<TPixel> source, Rectangle sourceRectangle, Color color, YugenColor myColor)
        {
            Source = source;

            _color = color;

            MyColor = myColor;
        }


        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source; // Avoid capturing this            

            for (int w = 0; w < source.Width; w++)
            {
                for (int h = 0; h < source.Height; h++)
                {
                    // Get current output color
                    Rgba32 pixel = new Rgba32();
                    source[w, h].ToRgba32(ref pixel);

                    var targetColor = ColorHelper.GetSolidColorBrush(_color.ToHex()).Color;

                    int R = Math.Min(255, Math.Max(0, (pixel.R + targetColor.R) / 2));
                    int G = Math.Min(255, Math.Max(0, (pixel.G + targetColor.G) / 2));
                    int B = Math.Min(255, Math.Max(0, (pixel.B + targetColor.B) / 2));

                    MyColor.R = R;
                    MyColor.G = G;
                    MyColor.B = B;

                    string hex = R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
                    var color = Rgba32.FromHex(hex);
                    source[w, h].FromRgba32(color);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
