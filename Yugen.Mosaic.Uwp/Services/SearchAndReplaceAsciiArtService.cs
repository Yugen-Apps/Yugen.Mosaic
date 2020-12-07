using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
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
            var text = GenerateText(masterImage, ratio);
            
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasTextFormat textFormat = new CanvasTextFormat()
            {
                FontFamily = "Courier New",
                FontSize = 14,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                WordWrapping = CanvasWordWrapping.NoWrap
            };
            var textLayout = new CanvasTextLayout(device, text, textFormat, 100, 100);

            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (float)textLayout.LayoutBounds.Width, 
                (float)textLayout.LayoutBounds.Height, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawTextLayout(textLayout, 0, 0, Colors.Black);
            }             

            return Generate(renderTarget).Result;
        }

        private string GenerateText(Image<Rgba32> masterImage, int ratio = 5)
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

            return sb.ToString();
        }

        // TODO: in x64 Release mode doesn't work
        // with the current SixLabors.ImageSharp.Drawing beta version
        //public Image<Rgba32> SearchAndReplace(Image<Rgba32> masterImage, int ratio = 5)
        //{
        //    var text = GenerateText(masterImage, ratio);
        //    var font = SystemFonts.CreateFont("Courier New", 14);
        //    var size = TextMeasurer.Measure(text, new RendererOptions(font));

        //    var finalImage = new Image<Rgba32>((int)size.Width, (int)size.Height);
        //    finalImage.Mutate(i =>
        //    {
        //        i.Fill(Color.White);
        //        i.DrawText(text, font, Color.Black, new PointF(0, 0));
        //    });

        //    return finalImage;
        //}

        private async Task<Image<Rgba32>> Generate(CanvasRenderTarget renderTarget)
        {
            Image<Rgba32> finalImage;
            IRandomAccessStream inputRandomAccessStream = new InMemoryRandomAccessStream();
            await renderTarget.SaveAsync(inputRandomAccessStream, CanvasBitmapFileFormat.Png);
            finalImage = Image.Load<Rgba32>(inputRandomAccessStream.AsStreamForRead());

            return finalImage;
        }
    }
}