using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class NewMosaicService
    {
        private List<NewTile> _tileList { get; set; } = new List<NewTile>();
        private Size _tileSize;

        private int _tX;
        private int _tY;
        private Color[,] _avgsMaster;

        private readonly BenchmarkHelper benchmarkHelper = new BenchmarkHelper();

        public Image GenerateMosaic(Image masterImage, Size outputSize, List<Image> tileImageList, Size tileSize, bool isAdjustHue)
        {
            _tileSize = tileSize;
            _tX = masterImage.Width / tileSize.Width;
            _tY = masterImage.Height / tileSize.Height;
            _avgsMaster = new Color[_tX, _tY];

            GetTilesAverage(masterImage);

            var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height);

            LoadTilesAndResize(tileImageList);

            SearchAndReplace(outputImage, tileSize, isAdjustHue);

            return outputImage;
        }
        
        private void GetTilesAverage(Image masterImage)
        {
            benchmarkHelper.Start();

            var getTilesAverageProcessor = new GetTilesAverageProcessor(_tX, _tY, _tileSize, _avgsMaster);
            masterImage.Mutate(c => c.ApplyProcessor(getTilesAverageProcessor));

            benchmarkHelper.Stop("2");
        }

        private void LoadTilesAndResize(List<Image> tileImageList)
        {
            //progressBarMaximum = tileBmpList.Count;
            //progressBarValue = 0;

            foreach (var image in tileImageList)
            {
                MyColor myColor = new MyColor();

                image.Mutate(x => x.Resize(_tileSize.Width, _tileSize.Height));
                var getTileAverageProcessor = new GetTileAverageProcessor(0, 0, image.Width, image.Height, myColor);
                image.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));

                _tileList.Add(new NewTile(image, myColor.ToColor));

                //progressBarValue++;
            }
        }

        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize, bool isAdjustHue)
        {
            if (_tileList.Count < 1)
                return;

            benchmarkHelper.Start();

            //progressBarMaximum = tX * tY;
            //progressBarValue = 0;

            if (isAdjustHue)
            {
                //SearchAndReplaceAdjustHue(outputImage, tileSize);
            }
            else
            {
                SearchAndReplace(outputImage, tileSize);
            }

            benchmarkHelper.Stop("5");
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
                    NewTile tFound = null;
                    while (tFound == null)
                    {
                        int index = r.Next(_tileList.Count);
                        if (GetDifference(_avgsMaster[x, y], _tileList[index].Color) < threshold)
                        {
                            tFound = _tileList[index];
                        }
                        else
                        {
                            searchCounter++;
                            if (searchCounter >= _tileList.Count) { threshold += 5; }
                        }
                    }

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            MyColor myColor = new MyColor();
                            var getPixelProcessor = new GetPixelProcessor(w, h, myColor);
                            tFound.Image.Mutate(c => c.ApplyProcessor(getPixelProcessor));

                            outputImage[x * tileSize.Width + w, y * tileSize.Height + h] = myColor.ToColor;
                            //outputImage.SetPixel(x * tileSize.Width + w, y * tileSize.Height + h, myColor);
                        }
                    }

                    //progressBarValue++;
                }
            }
        }

        public int GetDifference(Color source, Color target)
        {
            var SourceColor = GetSolidColorBrush(source.ToHex()).Color;
            var targetColor = GetSolidColorBrush(target.ToHex()).Color;

            int dR = Math.Abs(SourceColor.R - targetColor.R);
            int dG = Math.Abs(SourceColor.G - targetColor.G);
            int dB = Math.Abs(SourceColor.B - targetColor.B);
            int diff = Math.Max(dR, dG);
            diff = Math.Max(diff, dB);
            return diff;
        }

        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }
    }
}
