using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
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
        private List<Image<Rgba32>> _tileImageList = new List<Image<Rgba32>>();

        private List<Tile> _resizedTileList { get; set; } = new List<Tile>();
        private Size _tileSize;

        private int _tX;
        private int _tY;
        private Rgba32[,] _avgsMaster;


        public Image<Rgba32> AddMasterImage(Stream stream)
        {
            _masterImage = Image.Load<Rgba32>(stream);
            return _masterImage;
        }

        public Image<Rgba32> AddTileImage(Stream stream)
        {
            var image = Image.Load<Rgba32>(stream);
            _tileImageList.Add(image);
            return image;
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
            if (_masterImage == null || _tileImageList.Count < 1)
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
            //progressBarMaximum = tileBmpList.Count;
            //progressBarValue = 0;

            _resizedTileList.Clear();
            foreach (var image in _tileImageList)
            {                
                image.Mutate(x => x.Resize(_tileSize.Width, _tileSize.Height));
                var getTileAverageProcessor = new GetTileAverageProcessor(0, 0, image.Width, image.Height);
                image.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));

                _resizedTileList.Add(new Tile(image, getTileAverageProcessor.MyColor[0]));

                //progressBarValue++;
            }
        }


        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize, int mosaicType)
        {
            if (_resizedTileList.Count < 1)
                return;
            
            //progressBarMaximum = tX * tY;
            //progressBarValue = 0;

            switch (mosaicType)
            {
                case 0:
                    SearchAndReplace(outputImage, tileSize);
                    break;
                case 1:
                    SearchAndReplaceAdjustHue(outputImage, tileSize);
                    break;
                case 2:
                    PlainColor(outputImage, tileSize);
                    break;
            }
        }

        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();

            // Don't adjust hue - keep searching for a tile close enough
            for (int x = 0; x < _tX; x++)
            {
                for (int y = 0; y < _tY; y++)
                {
                    // Reset searching threshold
                    int threshold = 0;
                    int searchCounter = 0;
                    Tile tFound = null;
                    while (tFound == null)
                    {
                        int index = r.Next(_resizedTileList.Count);
                        var difference = GetDifference(_avgsMaster[x, y], _resizedTileList[index].Color);
                        if (difference < threshold)
                        {
                            tFound = _resizedTileList[index];
                        }
                        else
                        {
                            searchCounter++;
                            if (searchCounter >= _resizedTileList.Count)
                                threshold += 5;
                        }
                    }

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            var getPixelProcessor = new GetPixelProcessor(w, h);
                            tFound.Image.Mutate(c => c.ApplyProcessor(getPixelProcessor));

                            outputImage[x * tileSize.Width + w, y * tileSize.Height + h] = getPixelProcessor.MyColor[0];
                        }
                    }

                    //progressBarValue++;
                }
            }
        }
        
        private void SearchAndReplaceAdjustHue(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();
            // Adjust hue - get the first (random) tile found and adjust its colours
            // to suit the average
            List<Tile> tileQueue = new List<Tile>();
            int maxQueueLength = Math.Min(1000, Math.Max(0, _resizedTileList.Count - 50));

            for (int x = 0; x < _tX; x++)
            {
                for (int y = 0; y < _tY; y++)
                {
                    int index = 0;
                    // Check if it's the same as the last (X)?
                    if (tileQueue.Count > 1)
                    {
                        while (tileQueue.Contains(_resizedTileList[index]))
                        {
                            index = r.Next(_resizedTileList.Count);
                        }
                    }

                    // Add to the 'queue'
                    Tile tFound = _resizedTileList[index];
                    if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0)
                        tileQueue.RemoveAt(0);
                    tileQueue.Add(tFound);

                    // Adjust the hue
                    Image<Rgba32> adjustedImage = new Image<Rgba32>(tFound.Image.Width, tFound.Image.Height);
                    var adjustHueProcessor = new AdjustHueProcessor(tFound.Image, _avgsMaster[x, y]);
                    adjustedImage.Mutate(c => c.ApplyProcessor(adjustHueProcessor));

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            var getPixelProcessor = new GetPixelProcessor(w, h);
                            adjustedImage.Mutate(c => c.ApplyProcessor(getPixelProcessor));

                            outputImage[x * tileSize.Width + w, y * tileSize.Height + h] = getPixelProcessor.MyColor[0];
                        }
                    }

                    //progressBarValue++;
                }
            }
        }


        private void PlainColor(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();
            // Adjust hue - get the first (random) tile found and adjust its colours
            // to suit the average
            List<Tile> tileQueue = new List<Tile>();
            int maxQueueLength = Math.Min(1000, Math.Max(0, _resizedTileList.Count - 50));

            for (int x = 0; x < _tX; x++)
            {
                for (int y = 0; y < _tY; y++)
                {
                    int index = 0;
                    // Check if it's the same as the last (X)?
                    if (tileQueue.Count > 1)
                    {
                        while (tileQueue.Contains(_resizedTileList[index]))
                        {
                            index = r.Next(_resizedTileList.Count);
                        }
                    }

                    // Add to the 'queue'
                    Tile tFound = _resizedTileList[index];
                    if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0)
                        tileQueue.RemoveAt(0);
                    tileQueue.Add(tFound);

                    // Adjust the hue
                    Image<Rgba32> adjustedImage = new Image<Rgba32>(tFound.Image.Width, tFound.Image.Height);
                    var plainColorProcessor = new PlainColorProcessor(_avgsMaster[x, y]);
                    adjustedImage.Mutate(c => c.ApplyProcessor(plainColorProcessor));

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            var getPixelProcessor = new GetPixelProcessor(w, h);
                            adjustedImage.Mutate(c => c.ApplyProcessor(getPixelProcessor));

                            outputImage[x * tileSize.Width + w, y * tileSize.Height + h] = getPixelProcessor.MyColor[0];
                        }
                    }

                    //progressBarValue++;
                }
            }
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
