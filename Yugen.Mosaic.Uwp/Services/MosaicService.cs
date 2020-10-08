using Microsoft.Extensions.DependencyInjection;
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
using Yugen.Mosaic.Uwp.Interfaces;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Toolkit.Standard.Core.Models;
using Yugen.Toolkit.Standard.Services;
using Yugen.Toolkit.Uwp.Helpers;

namespace Yugen.Mosaic.Uwp.Services
{
    public class MosaicService : IMosaicService
    {
        private readonly IProgressService _progressService;

        private Rgba32[,] _avgsMaster;
        private int _tX;
        private int _tY;
        private readonly List<Tile> _tileImageList = new List<Tile>();
        private Image<Rgba32> _masterImage;
        private Size _tileSize;
        private ISearchAndReplaceService _searchAndReplaceService;
        private readonly ISearchAndReplaceAsciiArtService _searchAndReplaceAsciiArtService;

        public MosaicService(IProgressService progressService, ISearchAndReplaceAsciiArtService searchAndReplaceAsciiArtService)
        {
            _progressService = progressService;
            _searchAndReplaceAsciiArtService = searchAndReplaceAsciiArtService;
        }

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

        public async Task<Result<Image<Rgba32>>> GenerateMosaic(Size outputSize, Size tileSize, MosaicTypeEnum selectedMosaicType)
        {
            if (_masterImage == null)
            {
                var message = ResourceHelper.GetText("MosaicServiceErrorMasterImage");
                return Result.Fail<Image<Rgba32>>(message);
            }

            if (selectedMosaicType != MosaicTypeEnum.PlainColor &&
                selectedMosaicType != MosaicTypeEnum.AsciiArt &&
                _tileImageList.Count < 1)
            {
                var message = ResourceHelper.GetText("MosaicServiceErrorTiles");
                return Result.Fail<Image<Rgba32>>(message);
            }

            Image<Rgba32> resizedMasterImage = _masterImage.Clone(x => x.Resize(outputSize.Width, outputSize.Height));

            if (selectedMosaicType == MosaicTypeEnum.AsciiArt)
            {
                var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height);
                _searchAndReplaceAsciiArtService.Init(resizedMasterImage, outputImage);
                
                GC.Collect();

                return Result.Ok(_searchAndReplaceAsciiArtService.SearchAndReplace());
            }
            else
            {
                _tileSize = tileSize;
                _tX = resizedMasterImage.Width / tileSize.Width;
                _tY = resizedMasterImage.Height / tileSize.Height;
                _avgsMaster = new Rgba32[_tX, _tY];

                GetTilesAverage(resizedMasterImage);

                if (selectedMosaicType != MosaicTypeEnum.PlainColor)
                {
                    await LoadTilesAndResize();
                }

                return Result.Ok(SearchAndReplace(tileSize, selectedMosaicType, outputSize));
            }
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
            Parallel.For(0, _tY, y =>
            {
                Span<Rgba32> rowSpan = masterImage.GetPixelRowSpan(y);

                for (var x = 0; x < _tX; x++)
                {
                    _avgsMaster[x, y].FromRgba32(Helpers.ColorHelper.GetAverageColor(masterImage, x, y, _tileSize));
                }

                _progressService.IncrementProgress(_tY, 0, 33);
            });
        }

        private async Task LoadTilesAndResize()
        {
            _progressService.Reset();

            var processTiles = _tileImageList.AsParallel().Select(tile => ProcessTile(tile));

            await Task.WhenAll(processTiles);
        }

        private async Task ProcessTile(Tile tile)
        {
            await tile.Process(_tileSize);

            _progressService.IncrementProgress(_tileImageList.Count, 33, 66);
        }

        private Image<Rgba32> SearchAndReplace(Size tileSize, MosaicTypeEnum selectedMosaicType, Size outputSize)
        {
            var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height);

            switch (selectedMosaicType)
            {
                case MosaicTypeEnum.Classic:
                    _searchAndReplaceService = AppContainer.Services.GetService<SearchAndReplaceClassicService>();
                    break;

                case MosaicTypeEnum.Random:
                    _searchAndReplaceService = AppContainer.Services.GetService<SearchAndReplaceRandomService>();
                    break;

                case MosaicTypeEnum.AdjustHue:
                    _searchAndReplaceService = AppContainer.Services.GetService<SearchAndReplaceAdjustHueService>();
                    break;

                case MosaicTypeEnum.PlainColor:
                    _searchAndReplaceService = AppContainer.Services.GetService<SearchAndReplacePlainColorService>();
                    break;
            }

            _searchAndReplaceService.Init(_avgsMaster, outputImage, _tileImageList, tileSize, _tX, _tY);
            _searchAndReplaceService.SearchAndReplace();

            GC.Collect();

            return outputImage;
        }
    }
}