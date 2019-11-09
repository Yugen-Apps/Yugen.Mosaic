using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService
    {
        private List<Tile> TileBmpList { get; set; } = new List<Tile>();

        private int tileSizeWidth;
        private int tileSizeHeight;

        private int tX;
        private int tY;
        private Color[,] avgsMaster;

        private int progressBarMaximum;
        private int progressBarValue;

        private readonly BenchmarkHelper benchmarkHelper = new BenchmarkHelper();

        public LockBitmap GenerateMosaic(WriteableBitmap masterBmp, Size outputSize, List<WriteableBitmap> tileBmpList, Size tileSize, bool isAdjustHue)
        {
            tileSizeWidth = (int)tileSize.Width;
            tileSizeHeight = (int)tileSize.Height;

            tX = masterBmp.PixelWidth / tileSizeWidth;
            tY = masterBmp.PixelHeight / tileSizeHeight;
            avgsMaster = new Color[tX, tY];

            GetTilesAverage(masterBmp);

            var outputBmp = new LockBitmap(outputSize);

            LoadTilesAndResize(tileBmpList);

            SearchAndReplace(isAdjustHue, tileSize, outputBmp);

            return outputBmp;
        }

        private void GetTilesAverage(WriteableBitmap masterBmp)
        {
            benchmarkHelper.Start();

            progressBarMaximum = tX * tY;
            progressBarValue = 0;

            for (int x = 0; x < tX; x++)
            {
                for (int y = 0; y < tY; y++)
                {
                    avgsMaster[x, y] = GetTileAverage(masterBmp, x * tileSizeWidth, y * tileSizeHeight, tileSizeWidth, tileSizeHeight);
                    progressBarValue++;
                }
            }

            benchmarkHelper.Stop("2");
        }             


        private void LoadTilesAndResize(List<WriteableBitmap> tileBmpList)
        {
            progressBarMaximum = tileBmpList.Count;
            progressBarValue = 0;

            foreach (var file in tileBmpList)
            {
                var bmp = file.Resize(tileSizeWidth, tileSizeHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                var color = GetTileAverage(bmp, 0, 0, bmp.PixelWidth, bmp.PixelHeight);
                TileBmpList.Add(new Tile(bmp, color));
                progressBarValue++;
            }
        }


        private void SearchAndReplace(bool isAdjustHue, Size tileSize, LockBitmap outputBmp)
        {
            if (TileBmpList.Count < 1)
                return;

            benchmarkHelper.Start();

            progressBarMaximum = tX * tY;
            progressBarValue = 0;

            if (isAdjustHue)
            {
                SearchAndReplaceAdjustHue(tileSize, outputBmp);
            }
            else
            {
                SearchAndReplace(tileSize, outputBmp);
            }

            benchmarkHelper.Stop("5");
        }

        private void SearchAndReplace(Size tileSize, LockBitmap outputBmp)
        {
            Random r = new Random();
            // Don't adjust hue - keep searching for a tile close enough
            for (int x = 0; x < tX; x++)
            {
                for (int y = 0; y < tY; y++)
                {
                    // Reset searching threshold
                    int threshold = 0;
                    int searchCounter = 0;
                    Tile tFound = null;
                    while (tFound == null)
                    {
                        int index = r.Next(TileBmpList.Count);
                        var difference = GetDifference(avgsMaster[x, y], TileBmpList[index].color);
                        if (difference < threshold)
                        {
                            tFound = TileBmpList[index];
                        }
                        else
                        {
                            searchCounter++;
                            if (searchCounter >= TileBmpList.Count) 
                                threshold += 5;
                        }
                    }

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            var color = tFound.bitmap.GetPixel(w, h);
                            outputBmp.SetPixel(x * tileSizeWidth + w, y * tileSizeHeight + h, color);
                        }
                    }

                    progressBarValue++;
                }
            }
        }

        private void SearchAndReplaceAdjustHue(Size tileSize, LockBitmap outputBmp)
        {
            Random r = new Random();
            // Adjust hue - get the first (random) tile found and adjust its colours
            // to suit the average
            List<Tile> tileQueue = new List<Tile>();
            int maxQueueLength = Math.Min(1000, Math.Max(0, TileBmpList.Count - 50));

            for (int x = 0; x < tX; x++)
            {
                for (int y = 0; y < tY; y++)
                {
                    int index = 0;
                    // Check if it's the same as the last (X)?
                    if (tileQueue.Count > 1)
                    {
                        while (tileQueue.Contains(TileBmpList[index]))
                        {
                            index = r.Next(TileBmpList.Count);
                        }
                    }

                    // Add to the 'queue'
                    Tile tFound = TileBmpList[index];
                    if (tileQueue.Count >= maxQueueLength && tileQueue.Count > 0) { tileQueue.RemoveAt(0); }
                    tileQueue.Add(tFound);

                    // Adjust the hue
                    WriteableBitmap bAdjusted = AdjustHue(tFound.bitmap, avgsMaster[x, y]);

                    // Apply found tile to section
                    for (int w = 0; w < tileSize.Width; w++)
                    {
                        for (int h = 0; h < tileSize.Height; h++)
                        {
                            var color = bAdjusted.GetPixel(w, h);
                            outputBmp.SetPixel(x * tileSizeWidth + w, y * tileSizeHeight + h, color);
                        }
                    }

                    progressBarValue++;
                }
            }
        }


        public static Color GetTileAverage(WriteableBitmap bSource, int x, int y, int width, int height)
        {
            long aR = 0;
            long aG = 0;
            long aB = 0;

            for (int w = x; w < x + width; w++)
            {
                for (int h = y; h < y + height; h++)
                {
                    var color = bSource.GetPixel(w, h);

                    aR += color.R;
                    aG += color.G;
                    aB += color.B;
                }
            }

            aR /= width * height;
            aG /= width * height;
            aB /= width * height;

            return Color.FromArgb(255, Convert.ToByte(aR), Convert.ToByte(aG), Convert.ToByte(aB));
        }

        public static int GetDifference(Color source, Color target)
        {
            int dR = Math.Abs(source.R - target.R);
            int dG = Math.Abs(source.G - target.G);
            int dB = Math.Abs(source.B - target.B);
            int diff = Math.Max(dR, dG);
            diff = Math.Max(diff, dB);
            return diff;
        }
        
        public static WriteableBitmap AdjustHue(WriteableBitmap bSource, Color targetColor)
        {
            WriteableBitmap result = BitmapFactory.New(bSource.PixelWidth, bSource.PixelHeight);

            for (int w = 0; w < bSource.PixelWidth; w++)
            {
                for (int h = 0; h < bSource.PixelHeight; h++)
                {
                    // Get current output color
                    var clSource = bSource.GetPixel(w, h);
                    int R = Math.Min(255, Math.Max(0, (clSource.R + targetColor.R) / 2));
                    int G = Math.Min(255, Math.Max(0, (clSource.G + targetColor.G) / 2));
                    int B = Math.Min(255, Math.Max(0, (clSource.B + targetColor.B) / 2));
                    Color clAvg = Color.FromArgb(255, Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B));

                    result.SetPixel(w, h, clAvg);
                }
            }
            return result;
        }
    }
}
