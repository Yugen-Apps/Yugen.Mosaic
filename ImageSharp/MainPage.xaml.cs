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
    }
}
