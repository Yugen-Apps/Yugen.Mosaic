using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;
using System;

namespace Yugen.Mosaic.Uwp.Processors
{
    public sealed class AdjustHueProcessor : IImageProcessor
    {
        public Image<Rgba32> Image { get; }
        public Color AverageColor { get; }

        public AdjustHueProcessor(Image<Rgba32> image, Color averageColor)
        {
            Image = image;
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

        private readonly Image<Rgba32> Image;
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
            Image = definition.Image;
            AverageColor = definition.AverageColor;
        }

        /// <inheritdoc/>
        public void Apply()
        {
            int width = Source.Width;
            Image<TPixel> source = Source;
            Rgba32 targetColor = AverageColor.ToPixel<Rgba32>();

            //Parallel.For(0, source.Height, y =>
            //{
            //    Vector4 averageColor4 = Unsafe.As<RgbaVector, Vector4>(ref averageColorVector);
            //    ref TPixel r0 = ref source.GetPixelRowSpan(y).GetPinnableReference();

            //    for (int x = 0; x < width; x++)
            //    {
            //        Vector4 color4 = Unsafe.Add(ref r0, x).ToVector4();
            //        color4 = (color4 + averageColor4) / 2;
            //        color4 = Vector4.Clamp(color4, Vector4.Zero, Vector4.One);

            //        Unsafe.Add(ref r0, x).FromVector4(color4);
            //    }
            //});

            for (int h = 0; h < Image.Height; h++)
            {
                for (int w = 0; w < Image.Width; w++)
                {

                    // Get current output color
                    //var clSource = bSource.GetPixel(w, h);
                    //Rgba32 colorSource = new Rgba32();
                    //source[w, h].ToRgba32(ref colorSource);


                    Rgba32 pixel = new Rgba32();
                    Image[w, h].ToRgba32(ref pixel);


                    int R = Math.Min(255, Math.Max(0, (pixel.R + targetColor.R) / 2));
                    int G = Math.Min(255, Math.Max(0, (pixel.G + targetColor.G) / 2));
                    int B = Math.Min(255, Math.Max(0, (pixel.B + targetColor.B) / 2));

                    //Color clAvg = Color.FromArgb(255, Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));
                    Rgba32 clAvg = new Rgba32(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B), 255);
                    //result.SetPixel(w, h, clAvg);

                    TPixel a = new TPixel();
                    a.FromRgba32(clAvg);
                    Source[w, h] = a;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
