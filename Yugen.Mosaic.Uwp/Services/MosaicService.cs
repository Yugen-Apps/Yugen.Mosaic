using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService : IMosaicService
    {
        internal Rgba32[,] _avgsMaster;
        internal int _tX;
        internal int _tY;
        internal List<Tile> _tileImageList = new List<Tile>();

        private Image<Rgba32> _masterImage;
        private Size _tileSize;

        public async Task<Size> AddMasterImage(StorageFile file)
        {
            using (var inputStream = await file.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                _masterImage = Image.Load<Rgba32>(stream);
            }

            return _masterImage.Size();
        }

        public void AddTileImage(string name, StorageFile file)
        {
            _tileImageList.Add(new Tile(name, file));
        }

        public async Task<Image<Rgba32>> GenerateMosaic(Size outputSize, Size tileSize, MosaicTypeEnum selectedMosaicType)
        {
            if (_masterImage == null || (selectedMosaicType != MosaicTypeEnum.PlainColor && _tileImageList.Count < 1))
            {
                return null;
            }

            Image<Rgba32> resizedMasterImage = _masterImage.Clone(x => x.Resize(outputSize.Width, outputSize.Height));

            _tileSize = tileSize;
            _tX = resizedMasterImage.Width / tileSize.Width;
            _tY = resizedMasterImage.Height / tileSize.Height;
            _avgsMaster = new Rgba32[_tX, _tY];

            GetTilesAverage(resizedMasterImage);

            if (selectedMosaicType != MosaicTypeEnum.PlainColor)
            {
                await LoadTilesAndResize();
            }

            return SearchAndReplace(tileSize, selectedMosaicType, outputSize);
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
            var outputStream = new InMemoryRandomAccessStream();
            image.SaveAsJpeg(outputStream.AsStreamForWrite());
            outputStream.Seek(0);
            return outputStream;
        }

        public void RemoveTileImage(string name)
        {
            Tile item = _tileImageList.FirstOrDefault(x => x.Name.Equals(name));
            if (item != null)
            {
                _tileImageList.Remove(item);
            }
        }

        public void Reset()
        {
            _masterImage = null;
            _tileImageList.Clear();
        }

        private void GetTilesAverage(Image<Rgba32> masterImage)
        {
            ProgressHelper.ResetProgress();

            Parallel.For(0, _tY, y =>
            {
                Span<Rgba32> rowSpan = masterImage.GetPixelRowSpan(y);

                for (var x = 0; x < _tX; x++)
                {
                    _avgsMaster[x, y].FromRgba32(ColorHelper.GetAverageColor(masterImage, x, y, _tileSize));
                }

                ProgressHelper.IncrementProgress(0, 33, _tY);
            });
        }

        private async Task LoadTilesAndResize()
        {
            ProgressHelper.ResetProgress();

            var processTiles = _tileImageList.AsParallel().Select(tile => ProcessTile(tile));

            await Task.WhenAll(processTiles);
        }

        private async Task ProcessTile(Tile tile)
        {
            await tile.Process(_tileSize);

            ProgressHelper.IncrementProgress(33, 33, _tileImageList.Count);
        }

        private Image<Rgba32> SearchAndReplace(Size tileSize, MosaicTypeEnum selectedMosaicType, Size outputSize)
        {
            var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height);

            ISearchAndReplaceService SearchAndReplaceService;

            switch (selectedMosaicType)
            {
                case MosaicTypeEnum.Classic:
                    SearchAndReplaceService = new ClassicSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;

                case MosaicTypeEnum.Random:
                    SearchAndReplaceService = new RandomSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;

                case MosaicTypeEnum.AdjustHue:
                    SearchAndReplaceService = new AdjustHueSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;

                case MosaicTypeEnum.PlainColor:
                    SearchAndReplaceService = new PlainColorSearchAndReplaceService(outputImage, tileSize, _tX, _tY, _tileImageList, _avgsMaster);
                    SearchAndReplaceService.SearchAndReplace();
                    break;
            }

            GC.Collect();

            return outputImage;
        }
    }
}