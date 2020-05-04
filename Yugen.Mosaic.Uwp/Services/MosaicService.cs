using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Size = SixLabors.ImageSharp.Size;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService
    {
        private Image<Rgba32> _masterImage;

        internal List<Tile> _tileImageList { get; set; } = new List<Tile>();
        private Size _tileSize;

        internal int _tX;
        internal int _tY;
        internal Rgba32[,] _avgsMaster;

        internal int _progress;
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
                    _avgsMaster[x, y].FromRgba32(ColorHelper.GetAverageColor(masterImage, x, y, _tileSize));
                }
            });
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
                tile.AverageColor = ColorHelper.GetAverageColor(tile.ResizedImage);

                _progress++;
            }
        }


        private void SearchAndReplace(Image<Rgba32> outputImage, Size tileSize, int mosaicType)
        {
            _progressMax = _tileImageList.Count;
            _progress = 0;

            ISearchAndReplaceService SearchAndReplaceService;

            switch (mosaicType)
            {
                case 0:
                    SearchAndReplaceService = new ClassicSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;
                case 1:
                    SearchAndReplaceService = new RandomSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;
                case 2:
                    SearchAndReplaceService = new AdjustHueSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;
                case 3:
                    SearchAndReplaceService = new PlainColorSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;
            }

            GC.Collect();
        }


        public void Reset()
        {
            _masterImage = null;
            _tileImageList.Clear();
        }
    }
}
