using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class NewMosaicService
    {
        public List<NewTile> TileList { get; set; } = new List<NewTile>();

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

                var color = Color.FromRgb(Convert.ToByte(myColor.R), Convert.ToByte(myColor.G), Convert.ToByte(myColor.B)); ;
                TileList.Add(new NewTile(image, color));

                //progressBarValue++;
            }
        }

    }
}
