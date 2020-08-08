using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using System.Threading.Tasks;
using Yugen.Toolkit.Standard.Services;

namespace Yugen.Mosaic.Uwp.Services
{
    public class SearchAndReplacePlainColorService : SearchAndReplaceService
    {
        public SearchAndReplacePlainColorService(IProgressService progressService) : base(progressService) { }

        public override void SearchAndReplace()
        {
            _progressService.Reset();

            int max = _tX * _tY;

            Parallel.For(0, _tX * _tY, xy =>
            {
                int y = xy / _tX;
                int x = xy % _tX;

                // Generate colored tile
                var adjustedImage = new Image<Rgba32>(_tileSize.Width, _tileSize.Height);
                var averageColor4 = _avgsMaster[x, y].ToVector4();

                adjustedImage.Mutate(c => c.ProcessPixelRowsAsVector4(row =>
                {
                    foreach (ref Vector4 pixel in row)
                    {
                        pixel = (pixel + averageColor4) / 2;
                    }
                }));

                // Apply found tile to section
                ApplyTileFound(x, y, adjustedImage);

                _progressService.IncrementProgress(max, 66, 100);
            });
        }
    }
}