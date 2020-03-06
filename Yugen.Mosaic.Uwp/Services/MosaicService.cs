using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Processors;

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
            var getTilesAverageProcessor = new GetTilesAverageProcessor(_tX, _tY, _tileSize, _avgsMaster);
            masterImage.Mutate(c => c.ApplyProcessor(getTilesAverageProcessor));
        }

        private void LoadTilesAndResize()
        {
            _progressMax = _tileImageList.Count;
            _progress = 0;

            foreach (var tile in _tileImageList)
            {
                var getTileAverageProcessor = new GetTileAverageProcessor(0, 0, _tileSize.Width, _tileSize.Height, tile.OriginalImage);
                tile.OriginalImage.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));

                tile.ResizedImage = getTileAverageProcessor.ResizedImage;
                tile.AverageColor = getTileAverageProcessor.AverageColor[0];

                _progress++;
            }
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
        }


        private void SearchAndReplaceClassic(Image<Rgba32> outputImage, Size tileSize)
        {
            Parallel.For(0, _tX, x =>
            {
                for (int y = 0; y < _tY; y++)
                {
                    Tile tileFound = _tileImageList[0];
                    var difference = 100;
                    int index = 0;

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
                    var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                    tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                    _progress++;
                }
            });
        }

        // Don't adjust hue - keep searching for a tile close enough
        private void SearchAndReplaceRandom(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();

            Parallel.For(0, _tX, x =>
            {
                for (int y = 0; y < _tY; y++)
                {
                    // Reset searching threshold
                    int threshold = 0;
                    int searchCounter = 0;
                    Tile tileFound = null;

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
                    var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                    tileFound.ResizedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                    _progress++;
                }
            });
        }

        // Adjust hue - get the first (random) tile found and adjust its colours to suit the average
        private void SearchAndReplaceAdjustHue(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();
            List<Tile> tileQueue = new List<Tile>();
            int maxQueueLength = Math.Min(1000, Math.Max(0, _tileImageList.Count - 50));

            Parallel.For(0, _tX, x =>
            {
                for (int y = 0; y < _tY; y++)
                {
                    int index = 0;
                    // Check if it's the same as the last (X)?
                    if (tileQueue.Count > 1)
                    {
                        while (tileQueue.Contains(_tileImageList[index]))
                        {
                            index = r.Next(_tileImageList.Count);
                        }
                    }

                    // Add to the 'queue'
                    Tile tileFound = _tileImageList[index];
                    if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0)
                        tileQueue.RemoveAt(0);
                    tileQueue.Add(tileFound);

                    // Adjust the hue
                    Image<Rgba32> adjustedImage = new Image<Rgba32>(tileFound.ResizedImage.Width, tileFound.ResizedImage.Height);
                    var adjustHueProcessor = new AdjustHueProcessor(tileFound.ResizedImage, _avgsMaster[x, y]);
                    adjustedImage.Mutate(c => c.ApplyProcessor(adjustHueProcessor));

                    // Apply found tile to section
                    var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                    adjustedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                    _progress++;
                }
            });
        }

        // Use just mosic colored tiles
        private void PlainColor(Image<Rgba32> outputImage, Size tileSize)
        {
            Parallel.For(0, _tX, x =>
            {
                for (int y = 0; y < _tY; y++)
                {
                    // Adjust the hue
                    Image<Rgba32> adjustedImage = new Image<Rgba32>(tileSize.Width, tileSize.Height);
                    var plainColorProcessor = new PlainColorProcessor(_avgsMaster[x, y]);
                    adjustedImage.Mutate(c => c.ApplyProcessor(plainColorProcessor));

                    // Apply found tile to section
                    var applyTileFoundProcessor = new ApplyTileFoundProcessor(x, y, tileSize.Width, tileSize.Height, outputImage);
                    adjustedImage.Mutate(c => c.ApplyProcessor(applyTileFoundProcessor));

                    _progress++;
                }
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
