using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageSharp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var font = SystemFonts.CreateFont("Courier New", 14);
            var text = "Hello World";
            var size = TextMeasurer.Measure(text, new RendererOptions(font));

            var finalImage = new Image<Rgba32>((int)size.Width, (int)size.Height);
            finalImage.Mutate(i =>
            {
                i.Fill(Color.White);
                i.DrawText(text, font, Color.Black, new PointF(0, 0));
            });

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.FileTypeChoices.Add("Picture", new List<string>() { ".jpg" });
            StorageFile storageFile = await savePicker.PickSaveFileAsync();

            using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                finalImage.SaveAsPng(stream.AsStreamForWrite());
            }

            var outputStream = new InMemoryRandomAccessStream();
            finalImage.SaveAsJpeg(outputStream.AsStreamForWrite());
            outputStream.Seek(0);
            
            BitmapImage OutputBmpSource = new BitmapImage();
            await OutputBmpSource.SetSourceAsync(outputStream);

            OutputImage.Source = OutputBmpSource;
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
    }
}
