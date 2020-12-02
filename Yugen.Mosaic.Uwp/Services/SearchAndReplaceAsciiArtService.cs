using Microsoft.Graphics.Canvas;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
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
            
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, size.Width, size.Height, 96);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);

                int i = 0;
                using (StringReader reader = new StringReader(text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ds.DrawText(line, new System.Numerics.Vector2(0, i), Colors.Black);
                        i += 14;
                        //        _progressService.IncrementProgress(totalProgress);
                    }
                }
            }

            return Generate(renderTarget).Result;
        }

        //private async void TestWin2D()
        //{
        //    Uri imageuri = new Uri("ms-appx:///Assets/HelloMyNameIs.jpg");
        //    StorageFile inputFile = await StorageFile.GetFileFromApplicationUriAsync(imageuri);
        //    BitmapDecoder imagedecoder;
        //    using (var imagestream = await inputFile.OpenAsync(FileAccessMode.Read))
        //    {
        //        imagedecoder = await BitmapDecoder.CreateAsync(imagestream);
        //    }

        //    CanvasDevice device = CanvasDevice.GetSharedDevice();
        //    CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, 200, 200, 96);
        //    using (var ds = renderTarget.CreateDrawingSession())
        //    {
        //        ds.Clear(Colors.White);

        //        CanvasBitmap image = await CanvasBitmap.LoadAsync(device, inputFile.Path, 96);
        //        ds.DrawImage(image);            
        //        ds.DrawText("hello world", new System.Numerics.Vector2(0, 0), Colors.Black);
        //    }

        //    string filename = "test1.png";
        //    StorageFolder pictureFolder = KnownFolders.SavedPictures;
        //    var file = await pictureFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        //    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
        //    {
        //        await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
        //    }
        //}

        //public Image<Rgba32> SearchAndReplace(Image<Rgba32> masterImage, int ratio = 5)
        //{
        //    _progressService.Reset();

        //    StringBuilder sb = new StringBuilder();
        //    int hRatio = ratio * 2;
        //    var totalProgress = masterImage.Height / hRatio;

        //    for (int h = 0; h < masterImage.Height; h += hRatio)
        //    {
        //        Span<Rgba32> rowSpan = masterImage.GetPixelRowSpan(h);

        //        for (var w = 0; w < masterImage.Width; w += ratio)
        //        {
        //            var grayColor = new L8();
        //            grayColor.FromRgba32(rowSpan[w]);
        //            int index = grayColor.PackedValue * 10 / 255;
        //            sb.Append(asciiChars[index]);
        //        }

        //        sb.AppendLine();

        //        _progressService.IncrementProgress(totalProgress);
        //    }

        //    var font = SystemFonts.CreateFont("Courier New", 14);
        //    var text = sb.ToString();
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