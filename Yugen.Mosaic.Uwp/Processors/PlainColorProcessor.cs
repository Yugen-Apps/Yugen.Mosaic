//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing.Processors;
//using System.Numerics;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;

//namespace Yugen.Mosaic.Uwp.Processors
//{
//    public sealed class PlainColorProcessor : IImageProcessor
//    {
//        public PlainColorProcessor(Rgba32 averageColor)
//        {
//            AverageColor = averageColor;
//        }

//        public Rgba32 AverageColor { get; }

//        /// <inheritdoc />
//        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
//        { 
//            return new PlainColorProcessor<TPixel>(configuration, this, source, sourceRectangle);
//        }
//    }

//    public class PlainColorProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
//    {
//        /// <summary>
//        /// The source <see cref="Image{TPixel}"/> instance to modify
//        /// </summary>
//        private readonly Image<TPixel> _source;
//        private readonly Rgba32 _averageColor;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="PlainColorProcessor"/> class
//        /// </summary>
//        /// <param name="definition">The <see cref="PlainColorProcessor"/> defining the processor parameters</param>
//        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
//        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
//        public PlainColorProcessor(Configuration configuration, PlainColorProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
//        {
//            _source = source;
//            _averageColor = definition.AverageColor;
//        }

//        public void Execute()
//        {
//            int width = _source.Width;
//            Image<TPixel> source = _source;
//            //RgbaVector averageColorVector = AverageColor.ToPixel<RgbaVector>();

//            var averageColor4 = _averageColor.ToVector4(); //var b = AverageColor.ToScaledVector4();

//            Parallel.For(0, source.Height, y =>
//            {
//                //Vector4 averageColor4 = Unsafe.As<RgbaVector, Vector4>(ref averageColorVector);
//                ref TPixel r0 = ref source.GetPixelRowSpan(y).GetPinnableReference();

//                for (int x = 0; x < width; x++)
//                {
//                    Vector4 color4 = Unsafe.Add(ref r0, x).ToVector4();
//                    color4 = (color4 + averageColor4) / 2;
//                    color4 = Vector4.Clamp(color4, Vector4.Zero, Vector4.One);

//                    Unsafe.Add(ref r0, x).FromVector4(color4);
//                }
//            });
//        }

//        /// <inheritdoc/>
//        public void Dispose() { }
//    }
//}


