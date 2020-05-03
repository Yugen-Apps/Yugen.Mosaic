using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Processors;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService
    {
        private Image<Rgba32> _masterImage;

        private List<Tile> _tileImageList { get; set; } = new List<Tile>();
        private Size _tileSize;

        private int _tX;
        private int _tY;
        private Rgba32[,] _avgsMaster;

        private int _progress;
        private int _progressMax;


        public Image<Rgba32> AddMasterImage(Stream stream)
        {
            _masterImage = Image.Load<Rgba32>(stream);
            return _masterImage;
        }

        public Image<Rgba32> AddTileImage(string name, Stream stream)
        {
            var image = Image.Load<Rgba32>(stream);
            _tileImageList.Add(new Tile(image, name));
            return image;
        }

        public void RemoveTileImage(string name)
        {
            var item = _tileImageList.FirstOrDefault(x => x.Name.Equals(name));
            if (item != null)
                _tileImageList.Remove(item);
        }

        public Image<Rgba32> GetResizedImage(Image<Rgba32> image, int size)
        {
            var resizeOptions = new ResizeOptions()
            {
                Mode = ResizeMode.Max,
                Size = new Size(size, size)
            };

            return image.Clone(x => x.Resize(resizeOptions));
        }

        public InMemoryRandomAccessStream GetStream(Image<Rgba32> image)
        {
            InMemoryRandomAccessStream outputStream = new InMemoryRandomAccessStream();
            image.SaveAsJpeg(outputStream.AsStreamForWrite());
            outputStream.Seek(0);
            return outputStream;
        }


        public Image<Rgba32> GenerateMosaic(Size outputSize, Size tileSize, int mosaicType)
        {
            if (_masterImage == null || (mosaicType != 2 && _tileImageList.Count < 1))
                return null;

            Image<Rgba32> resizedMasterImage = _masterImage.Clone(x => x.Resize(outputSize.Width, outputSize.Height));

            _tileSize = tileSize;
            _tX = resizedMasterImage.Width / tileSize.Width;
            _tY = resizedMasterImage.Height / tileSize.Height;
            _avgsMaster = new Rgba32[_tX, _tY];

            GetTilesAverage(resizedMasterImage);

            LoadTilesAndResize();

            var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height);
            SearchAndReplace(outputImage, tileSize, mosaicType);
            return outputImage;
        }


        private void GetTilesAverage(Image<Rgba32> masterImage)
        {
            //var getTilesAverageProcessor = new GetTilesAverageProcessor(_tX, _tY, _tileSize, _avgsMaster);
            //masterImage.Mutate(c => c.ApplyProcessor(getTilesAverageProcessor));

            Parallel.For(0, _tY, y =>
            {
                var rowSpan = masterImage.GetPixelRowSpan(y);

                for (int x = 0; x < _tX; x++)
                {
                    _avgsMaster[x, y].FromRgba32(GetTileAverage(masterImage, x * _tileSize.Width, y * _tileSize.Height, _tileSize.Width, _tileSize.Height));
                }
            });
        }

        private Rgba32 GetTileAverage(Image<Rgba32> source, int x, int y, int width, int height)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(y, y + height, h =>
            {
                var rowSpan = source.GetPixelRowSpan(h);

                for (int w = x; w < x + width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            });

            aR /= width * height;
            aG /= width * height;
            aB /= width * height;

            return new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }


        private void LoadTilesAndResize()
        {
            _progressMax = _tileImageList.Count;
            _progress = 0;

            foreach (var tile in _tileImageList)
            {
                //var getTileAverageProcessor = new GetTileAverageProcessor(0, 0, _tileSize.Width, _tileSize.Height, tile.OriginalImage);
                //tile.OriginalImage.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));
                //tile.ResizedImage = getTileAverageProcessor.ResizedImage;
                //tile.AverageColor = getTileAverageProcessor.AverageColor[0];

                tile.ResizedImage = tile.OriginalImage.CloneAs<Rgba32>(); ;
                tile.ResizedImage.Mutate(x => x.Resize(_tileSize.Width, _tileSize.Height));
                tile.AverageColor = GetAverageColor(tile.ResizedImage);

                _progress++;
            }
        }

        private Rgba32 GetAverageColor(Image<Rgba32> source)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            Parallel.For(0, source.Height, h =>
            {
                var rowSpan = source.GetPixelRowSpan(h);

                for (int w = 0; w < source.Width; w++)
                {
                    Rgba32 pixel = new Rgba32();
                    rowSpan[w].ToRgba32(ref pixel);

                    aR += pixel.R;
                    aG += pixel.G;
                    aB += pixel.B;
                }
            });

            aR /= source.Width * source.Height;
            aG /= source.Width * source.Height;
            aB /= source.Width * source.Height;

            return new Rgba32(Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }


        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize, int mosaicType)
        {
            _progressMax = _tileImageList.Count;
            _progress = 0;

            switch (mosaicType)
            {
                case 0:
                    SearchAndReplaceClassic(outputImage, tileSize);
                    break;
                case 1:
                    SearchAndReplaceRandom(outputImage, tileSize);
                    break;
                case 2:
                    SearchAndReplaceAdjustHue(outputImage, tileSize);
                    break;
                case 3:
                    PlainColor(outputImage, tileSize);
                    break;
            }

            GC.Collect();
        }


        private void SearchAndReplaceClassic(Image<Rgba32> outputImage, Size tileSize)
        {
            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                int index = 0;
                int difference = 100;
                Tile tileFound = _tileImageList[0];

                // Search for a tile with a similar color
                foreach (var tile in _tileImageList)
                {
                    var newDifference = GetDifference(_avgsMaster[x, y], _tileImageList[index].AverageColor);
                    if (newDifference < difference)
                    {
                        tileFound = _tileImageList[index];
                        difference = newDifference;
                    }
                    index++;
                }

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, tileFound.ResizedImage, outputImage);

                _progress++;
            });
        }

        private void ApplyTileFoundProcessor(int x, int y, Image<Rgba32> source, Image<Rgba32> destination)
        {
            destination.Mutate(c =>
            {
                var point = new Point(x * source.Width, y * source.Height);
                c.DrawImage(source, point, 1);
            });
        }

        // Don't adjust hue - keep searching for a tile close enough
        private void SearchAndReplaceRandom(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // Reset searching variables
                int threshold = 0;
                int searchCounter = 0;
                Tile tileFound = null;

                // Search for a tile with a similar color
                while (tileFound == null)
                {
                    int index = r.Next(_tileImageList.Count);
                    var difference = GetDifference(_avgsMaster[x, y], _tileImageList[index].AverageColor);
                    if (difference < threshold)
                    {
                        tileFound = _tileImageList[index];
                    }
                    else
                    {
                        searchCounter++;
                        if (searchCounter >= _tileImageList.Count)
                            threshold += 5;
                    }
                }

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, tileFound.ResizedImage, outputImage);

                _progress++;
            });
        }

        // Adjust hue - get the first (random) tile found and adjust its colours to suit the average
        private void SearchAndReplaceAdjustHue(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();
            List<Tile> tileQueue = new List<Tile>();
            //int maxQueueLength = Math.Min(1000, Math.Max(0, _tileImageList.Count - 50));

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // (R * ColCount) + C
                int index = ((y * _tX) + x) % _tileImageList.Count;

                // Check if it's the same as the last (X)?
                //if (tileQueue.Count > 1)
                //{
                //    while (tileQueue.Contains(_tileImageList[index]))
                //    {
                //        index = r.Next(_tileImageList.Count);
                //    }
                //}

                // Add to the 'queue'
                Tile tileFound = _tileImageList[index];
                //if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0)
                //    tileQueue.RemoveAt(0);
                //tileQueue.Add(tileFound);

                // Adjust the hue
                Image<Rgba32> adjustedImage = new Image<Rgba32>(tileFound.ResizedImage.Width, tileFound.ResizedImage.Height);

                //var adjustHueProcessor = new AdjustHueProcessor(tileFound.ResizedImage, _avgsMaster[x, y]);
                //adjustedImage.Mutate(c => c.ApplyProcessor(adjustHueProcessor));

                AdjustHue(tileFound.ResizedImage, adjustedImage, _avgsMaster[x, y]);

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //adjustedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, adjustedImage, outputImage);

                _progress++;
            });
        }

        private void AdjustHue(Image<Rgba32> source, Image<Rgba32> output, Rgba32 averageColor)
        {
            output.Mutate(c =>
            {
                Parallel.For(0, source.Height, h =>
                {
                    var rowSpan = source.GetPixelRowSpan(h);

                    for (int w = 0; w < source.Width; w++)
                    {
                        Rgba32 pixel = new Rgba32();
                        rowSpan[w].ToRgba32(ref pixel);

                        int R = Math.Min(255, Math.Max(0, (pixel.R + averageColor.R) / 2));
                        int G = Math.Min(255, Math.Max(0, (pixel.G + averageColor.G) / 2));
                        int B = Math.Min(255, Math.Max(0, (pixel.B + averageColor.B) / 2));

                        Color clAvg = new Rgba32(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

                        Rgba32 pixelColor = clAvg.ToPixel<Rgba32>();
                        output[w, h] = pixelColor;
                    }
                });
            });
        }

        // Use just mosic colored tiles
        private void PlainColor(Image<Rgba32> outputImage, Size tileSize)
        {
            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // Generate colored tile
                Image<Rgba32> adjustedImage = new Image<Rgba32>(tileSize.Width, tileSize.Height);
                //var plainColorProcessor = new PlainColorProcessor(_avgsMaster[x, y]);
                //adjustedImage.Mutate(c => c.ApplyProcessor(plainColorProcessor));

                Vector4 averageColor4 = _avgsMaster[x, y].ToVector4();

                adjustedImage.Mutate(c => c.ProcessPixelRowsAsVector4(row =>
                {
                    foreach (ref Vector4 pixel in row)
                    {
                        pixel = (pixel + averageColor4) / 2;
                    }
                }));

                // Apply found tile to section
                //var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                //adjustedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                this.ApplyTileFoundProcessor(x, y, adjustedImage, outputImage);

                _progress++;
            });
        }


        private int GetDifference(Rgba32 source, Rgba32 target)
        {
            int dR = Math.Abs(source.R - target.R);
            int dG = Math.Abs(source.G - target.G);
            int dB = Math.Abs(source.B - target.B);
            int diff = Math.Max(dR, dG);
            return Math.Max(diff, dB);
        }


        public void Reset()
        {
            _masterImage = null;
            _tileImageList.Clear();
        }
    }
}
