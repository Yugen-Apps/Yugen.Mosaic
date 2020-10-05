using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Text;
using SixLabors.Fonts;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public class SearchAndReplaceAsciiArtService : ISearchAndReplaceAsciiArtService
    {
        private static string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
        private readonly IProgressService _progressService;

        private Image<Rgba32> _resizedMasterImage;
        private Image<Rgba32> _outputImage;

        public SearchAndReplaceAsciiArtService(IProgressService progressService)
        {
            _progressService = progressService;
        }

        public void Init(Image<Rgba32> resizedMasterImage, Image<Rgba32> outputImage)
        {
            _resizedMasterImage = resizedMasterImage;
            _outputImage = outputImage;
        }

        public Image<Rgba32> SearchAndReplace(int ratio = 5)
        {
            _progressService.Reset();

            bool toggle = false;
            StringBuilder sb = new StringBuilder();

            for (int h = 0; h < _resizedMasterImage.Height; h += ratio)
            {
                for (int w = 0; w < _resizedMasterImage.Width; w += ratio)
                {
                    var pixelColor = _resizedMasterImage[w, h];
                    var color = Convert.ToByte((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    var grayColor = new Rgba32(color, color, color);

                    if (!toggle)
                    {
                        int index = grayColor.R * 10 / 255;
                        sb.Append(asciiChars[index]);
                    }
                }

                if (!toggle)
                {
                    sb.AppendLine();
                    toggle = true;
                }
                else
                {
                    toggle = false;
                }

                _progressService.IncrementProgress(_resizedMasterImage.Height);
            }

            var font = SystemFonts.CreateFont("Arial", 10);
            var text = sb.ToString();
            var size = TextMeasurer.Measure(text, new RendererOptions(font));

            var finalImage = new Image<Rgba32>((int)size.Width, (int)size.Height);
            finalImage.Mutate(x => x.DrawText(text, font, Color.White, new PointF(0, 0)));

            //_outputImage = finalImage.Clone(x => x.Resize(_outputImage.Width, _outputImage.Height));

            return finalImage;
        }
    }
}