using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Text;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public class SearchAndReplaceAsciiArtService : ISearchAndReplaceAsciiArtService
    {
        private static readonly string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
        private readonly IProgressService _progressService;

        public SearchAndReplaceAsciiArtService(IProgressService progressService)
        {
            _progressService = progressService;
        }

        public Image<Rgba32> SearchAndReplace(Image<Rgba32> masterImage, int ratio = 5)
        {
            _progressService.Reset();

            StringBuilder sb = new StringBuilder();
            int hRatio = ratio * 2;
            var totalProgress = masterImage.Height / hRatio;

            for (int h = 0; h < masterImage.Height; h += hRatio)
            {
                Span<Rgba32> rowSpan = masterImage.GetPixelRowSpan(h);

                for (var w = 0; w < masterImage.Width; w += ratio)
                {
                    var grayColor = new L8();
                    grayColor.FromRgba32(rowSpan[w]);
                    int index = grayColor.PackedValue * 10 / 255;
                    sb.Append(asciiChars[index]);
                }

                sb.AppendLine();

                _progressService.IncrementProgress(totalProgress);
            }

            var font = SystemFonts.CreateFont("Courier New", 14);
            var text = sb.ToString();
            var size = TextMeasurer.Measure(text, new RendererOptions(font));

            var finalImage = new Image<Rgba32>((int)size.Width, (int)size.Height);
            finalImage.Mutate(i =>
            {
                i.Fill(Color.White);
                i.DrawText(text, font, Color.Black, new PointF(0, 0));
            });

            return finalImage;
        }
    }
}