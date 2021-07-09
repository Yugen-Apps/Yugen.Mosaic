using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using Windows.Storage.Streams;
using Yugen.Mosaic.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile : IDisposable
    {
        public Tile(string name, string faToken)
        {
            Name = name;
            FaToken = faToken;
        }

        public string Name { get; set; }

        public string FaToken { get; set; }

        public Image<Rgba32> ResizedImage { get; set; }

        public Rgba32 AverageColor { get; set; }

        public void Process(Size tileSize, IRandomAccessStream RandomAccessStream)
        {
            using (var stream = RandomAccessStream.AsStreamForRead())
            {
                ResizedImage = Image.Load<Rgba32>(stream);
            }
            ResizedImage.Mutate(x => x.Resize(tileSize));

            if (AverageColor == default)
            {
                AverageColor = ColorHelper.GetAverageColor(ResizedImage);
            }
        }

        public void Dispose()
        {
            ResizedImage.Dispose();
            ResizedImage = null;
        }
    }
}