using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public Image<Rgba32> Image { get; set; }
        public Rgba32 Color { get; set; }

        public Tile(Image<Rgba32> image, Rgba32 color)
        {
            Image = image;
            Color = color;
        }
    }
}

//Rgba32 targetColor = _averageColor.ToPixel<Rgba32>();

//Color clAvg = Color.FromRgba(255, Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));
//Rgba32 clAvg = Color.FromRgba(255, Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));
//Rgba32 clAvg = new Rgba32(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B), 255);

//TPixel pixelColor = new TPixel();
//pixelColor.FromRgba32(clAvg);

//public async Task RunTasks(WriteableBitmap clone)
//{
//    var tasks = new List<Task>();

//    tasks.Add(Task.Run(() => DoWork(400, 1, clone)));
//    tasks.Add(Task.Run(() => DoWork(200, 2, clone)));
//    tasks.Add(Task.Run(() => DoWork(300, 3, clone)));

//    await Task.WhenAll(tasks);
//}

//public async Task DoWork(int delay, int n, WriteableBitmap masterImageSource)
//{
//    await Task.Delay(delay);
//    System.Diagnostics.Debug.WriteLine($"{n} {masterImageSource.PixelHeight}");
//}
