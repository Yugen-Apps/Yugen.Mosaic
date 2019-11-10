using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Processors;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService
    {
        private List<Tile> _tileList { get; set; } = new List<Tile>();
        private Size _tileSize;

        private int _tX;
        private int _tY;
        private Color[,] _avgsMaster;
        
        public Image<Rgba32> GenerateMosaic(Image<Rgba32> masterImage, Size outputSize, List<Image<Rgba32>> tileImageList, Size tileSize, bool isAdjustHue)
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
        
        private void GetTilesAverage(Image<Rgba32> masterImage)
        {
            var getTilesAverageProcessor = new GetTilesAverageProcessor(_tX, _tY, _tileSize, _avgsMaster);
            masterImage.Mutate(c => c.ApplyProcessor(getTilesAverageProcessor));
        }

        private void LoadTilesAndResize(List<Image<Rgba32>> tileImageList)
        {
            //progressBarMaximum = tileBmpList.Count;
            //progressBarValue = 0;

            foreach (var image in tileImageList)
            {                
                image.Mutate(x => x.Resize(_tileSize.Width, _tileSize.Height));
                var getTileAverageProcessor = new GetTileAverageProcessor(0, 0, image.Width, image.Height);
                image.Mutate(c => c.ApplyProcessor(getTileAverageProcessor));

                _tileList.Add(new Tile(image, getTileAverageProcessor.MyColor[0]));

                //progressBarValue++;
            }
        }

        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize, bool isAdjustHue)
        {
            if (_tileList.Count < 1)
                return;
            
            //progressBarMaximum = tX * tY;
            //progressBarValue = 0;

            if (isAdjustHue)
            {
                SearchAndReplaceAdjustHue(outputImage, tileSize);
            }
            else
            {
                SearchAndReplace(outputImage, tileSize);
            }
        }

        private void SearchAndReplaceAdjustHue(Image<Rgba32> outputImage, Size tileSize)
        {
            Random r = new Random();
            // Adjust hue - get the first (random) tile found and adjust its colours
            // to suit the average
            List<Tile> tileQueue = new List<Tile>();
            int maxQueueLength = Math.Min(1000, Math.Max(0, _tileList.Count - 50));

            for (int x = 0; x < _tX; x++)
            {
                for (int y = 0; y < _tY; y++)
                {
                    int index = 0;
                    // Check if it's the same as the last (X)?
                    if (tileQueue.Count > 1)
                    {
                        while (tileQueue.Contains(_tileList[index]))
                        {
                            index = r.Next(_tileList.Count);
                        }
                    }

                    // Add to the 'queue'
                    Tile tFound = _tileList[index];
                    if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0) 
                        tileQueue.RemoveAt(0);
                    tileQueue.Add(tFound);

                    // Adjust the hue
                    Image<Rgba32> adjustedImage =  new Image<Rgba32>(tFound.Image.Width, tFound.Image.Height);
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
                        int index = r.Next(_tileList.Count);
                        var difference = GetDifference(_avgsMaster[x, y], _tileList[index].Color);
                        if (difference < threshold)
                        {
                            tFound = _tileList[index];
                        }
                        else
                        {
                            searchCounter++;
                            if (searchCounter >= _tileList.Count) 
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

        public int GetDifference(Color source, Color target)
        {
            var SourceColor = ColorHelper.GetSolidColorBrush(source.ToHex()).Color;
            var targetColor = ColorHelper.GetSolidColorBrush(target.ToHex()).Color;

            int dR = Math.Abs(SourceColor.R - targetColor.R);
            int dG = Math.Abs(SourceColor.G - targetColor.G);
            int dB = Math.Abs(SourceColor.B - targetColor.B);
            int diff = Math.Max(dR, dG);
            return Math.Max(diff, dB);
        }
    }
}
