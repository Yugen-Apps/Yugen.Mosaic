using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Toolkit.Standard.Core.Models;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface IMosaicService
    {
        Task<Size> AddMasterImage(StorageFile file);

        void AddTileImage(string name, StorageFile file);

        Task<Result<Image<Rgba32>>> GenerateMosaic(Size outputSize, Size tileSize, MosaicTypeEnum selectedMosaicType);

        Image<Rgba32> GetResizedImage(Image<Rgba32> image, int size);

        InMemoryRandomAccessStream GetStream(Image<Rgba32> image);

        void RemoveTileImage(string name);

        void Reset();
    }
}