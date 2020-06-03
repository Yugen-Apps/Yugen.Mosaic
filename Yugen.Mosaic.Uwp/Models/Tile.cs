using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Yugen.Mosaic.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.Models
{
    public class Tile
    {
        public Tile(string name, StorageFile file)
        {
            Name = name;
            File = file;
        }

        public string Name { get; set; }
        public StorageFile File { get; set; }
        public Image<Rgba32> ResizedImage { get; set; }
        public Rgba32 AverageColor { get; set; }

        public async Task Process(Size tileSize)
        {
            using (var inputStream = await File.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                ResizedImage = Image.Load<Rgba32>(stream);
            }

            ResizedImage.Mutate(x => x.Resize(tileSize));
            AverageColor = ColorHelper.GetAverageColor(ResizedImage);
        }
    }
}