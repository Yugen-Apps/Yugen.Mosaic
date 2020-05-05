//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
//using SixLabors.ImageSharp.Processing.Processors;
//using System;
//using System.Threading.Tasks;
//using Rectangle = SixLabors.ImageSharp.Rectangle;

//namespace Yugen.Mosaic.Uwp.Processors
//{
//    public sealed class GetTileAverageProcessor : IImageProcessor
//    {
//        public int X { get; }
//        public int Y { get; }
//        public int Width { get; }
//        public int Height { get; }

//        public Image<Rgba32> ResizedImage { get; }
//        public Rgba32[] AverageColor { get; } = new Rgba32[1];

//        public GetTileAverageProcessor(int x, int y, int width, int height, Image<Rgba32> resizedImage)
//        {
//            X = x;
//            Y = y;
//            Width = width;
//            Height = height;
//            ResizedImage = resizedImage.CloneAs<Rgba32>();
//        }

//        /// <inheritdoc/>
//        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
//        {
//            return new GetTileAverageProcessor<TPixel>(configuration, this, source, sourceRectangle);
//        }
//    }

//    public class GetTileAverageProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
//    {
//        /// <summary>
//        /// The source <see cref="Image{TPixel}"/> instance to modify
//        /// </summary>
//        private readonly Image<TPixel> _source;

//        private readonly int _x;
//        private readonly int _y;
//        private readonly int _width;
//        private readonly int _height;

//        private readonly Image<Rgba32> _resizedImage;
//        private readonly Rgba32[] _averageColor;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="HlslGaussianBlurProcessor"/> class
//        /// </summary>
//        /// <param name="definition">The <see cref="HlslGaussianBlurProcessor"/> defining the processor parameters</param>
//        /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance</param>
//        /// <param name="sourceRectangle">The source area to process for the current processor instance</param>
//        public GetTileAverageProcessor(Configuration configuration, GetTileAverageProcessor definition, Image<TPixel> source, Rectangle sourceRectangle)
//        {
//            _source = source;

//            _x = definition.X;
//            _y = definition.Y;
//            _width = definition.Width;
//            _height = definition.Height;

//            _resizedImage = definition.ResizedImage;
//            _averageColor = definition.AverageColor;
//        }

//        /// <inheritdoc/>
//        public void Execute()
//        {
//            _resizedImage.Mutate(x => x.Resize(_width, _height));

//            long aR = 0;
//            long aG = 0;
//            long aB = 0;

//            Parallel.For(_y, _y+_height, h =>
//            {
//                var rowSpan = _resizedImage.GetPixelRowSpan(h);

//                for (int w = _x; w < _x + _width; w++)
//                {
//                    Rgba32 pixel = new Rgba32();
//                    rowSpan[w].ToRgba32(ref pixel);

//                    aR += pixel.R;
//                    aG += pixel.G;
//                    aB += pixel.B;
//                }
//            });

//            aR /= _width * _height;
//            aG /= _width * _height;
//            aB /= _width * _height;

//            _averageColor[0] = new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
//        }

//        /// <inheritdoc/>
//        public void Dispose() { }
//    }
//}
