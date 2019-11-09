using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class AdjustHueProcessor : IImageProcessor
    {
        public Color AverageColor { get; }

        public AdjustHueProcessor(Color averageColor)
        {
            AverageColor = averageColor;
        }

        /// <inheritdoc/>
        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new AdjustHueProcessor<TPixel>(this, source, sourceRectangle);
        }
    }

    public class AdjustHueProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : struct, IPixel<TPixel>
    {
        /// <summary>
        /// The source <see cref="Image{TPixel}"/> instance to modify
        /// </summary>
        private readonly Image<TPixel> Source;

        private readonly Color AverageColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustHueProcessor"/> class
        /// </summary>
        /// <param name="definition">The <see cref="AdjustHueProcessor"/> defining the processor parameters</param>
        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
        public AdjustHueProcessor(AdjustHueProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
        {
            Source = source;
            AverageColor = definition.AverageColor;
        }


        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source;
            RgbaVector averageColorVector = AverageColor.ToPixel<RgbaVector>();

            Parallel.For(0, source.Height, y =>
            {
                Vector4 averageColor4 = Unsafe.As<RgbaVector, Vector4>(ref averageColorVector);
                ref TPixel r0 = ref source.GetPixelRowSpan(y).GetPinnableReference();

                for (int x = 0; x < width; x++)
                {
                    Vector4 color4 = Unsafe.Add(ref r0, x).ToVector4();
                    color4 = (color4 + averageColor4) / 2;
                    color4 = Vector4.Clamp(color4, Vector4.Zero, Vector4.One);

                    Unsafe.Add(ref r0, x).FromVector4(color4);
                }
            });
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
