using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Yugen.Mosaic.Uwp.Models
{
    public class YugenColor
    {
        public Rgba32 ClAvg { get; set; }
    }

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
}
