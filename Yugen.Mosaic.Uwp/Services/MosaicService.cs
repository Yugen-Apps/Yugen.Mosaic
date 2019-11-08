using System;
using System.Collections.Generic;
using System.Drawing;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Models;
using Color = System.Drawing.Color;
using ColorHelper = Yugen.Mosaic.Uwp.Helpers.ColorHelper;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService
    {
        public List<Tile> TileBmpList { get; set; } = new List<Tile>();

        public LockBitmap GenerateMosaic(WriteableBitmap masterBmp, Size outputSize, List<WriteableBitmap> tileBmpList, Size tileSize, bool isAdjustHue = false)
        {
            /// Average Master Image Phase
            int tX = masterBmp.PixelWidth / tileSize.Width;
            int tY = masterBmp.PixelHeight / tileSize.Height;
            Color[,] avgsMaster = new Color[tX, tY];

            for (int x = 0; x < tX; x++)
            {
                for (int y = 0; y < tY; y++)
                {
                    avgsMaster[x, y] = GetTileAverage(masterBmp, x * tileSize.Width, y * tileSize.Height, tileSize.Width, tileSize.Height);
                }
            }

            var outputBmp = new LockBitmap(outputSize);

            /// Tile Load And Resize Phase
            foreach (var file in tileBmpList)
            {
                var bmp = file.Resize(tileSize.Width, tileSize.Height, WriteableBitmapExtensions.Interpolation.Bilinear);
                var color = GetTileAverage(bmp, 0, 0, bmp.PixelWidth, bmp.PixelHeight);
                TileBmpList.Add(new Tile(bmp, color));
            }

            /// Iterative Replacement Phase / Search Phase
            if (TileBmpList.Count > 0)
            {
                Random r = new Random();
                if (isAdjustHue)
                {
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
                                    var newColor = ColorHelper.Convert(color);
                                    outputBmp.SetPixel(x * tileSize.Width + w, y * tileSize.Height + h, newColor);
                                }
                            }
                        }
                    }
                }
                else
                {
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
                                if (GetDifference(avgsMaster[x, y], TileBmpList[index].color) < threshold)
                                {
                                    tFound = TileBmpList[index];
                                }
                                else
                                {
                                    searchCounter++;
                                    if (searchCounter >= TileBmpList.Count) { threshold += 5; }
                                }
                            }
                            // Apply found tile to section
                            for (int w = 0; w < tileSize.Width; w++)
                            {
                                for (int h = 0; h < tileSize.Height; h++)
                                {
                                    var color = tFound.bitmap.GetPixel(w, h);
                                    var newColor = ColorHelper.Convert(color);
                                    outputBmp.SetPixel(x * tileSize.Width + w, y * tileSize.Height + h, newColor);
                                }
                            }
                        }
                    }
                }
            }

            return outputBmp;
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
                    var newColor = ColorHelper.Convert(color);

                    Color cP = newColor;
                    aR += cP.R;
                    aG += cP.G;
                    aB += cP.B;
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
                    Color clAvg = Color.FromArgb(R, G, B);

                    var newColor = ColorHelper.Convert(clAvg);
                    result.SetPixel(w, h, newColor);
                }
            }
            return result;
        }
    }
}
