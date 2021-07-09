using System;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Interfaces;

namespace Yugen.Mosaic.Uwp.Services
{
    public class SearchAndReplaceServiceFactory : ISearchAndReplaceServiceFactory
    {
        private readonly Func<SearchAndReplaceAdjustHueService> _adjusthue;
        private readonly Func<SearchAndReplaceClassicService> _classic;
        private readonly Func<SearchAndReplacePlainColorService> _plainColor;
        private readonly Func<SearchAndReplaceRandomService> _random;

        public SearchAndReplaceServiceFactory(
            Func<SearchAndReplaceAdjustHueService> adjusthue,
            Func<SearchAndReplaceClassicService> classic,
            Func<SearchAndReplacePlainColorService> plainColor,
            Func<SearchAndReplaceRandomService> random)
        {
            _adjusthue = adjusthue;
            _classic = classic;
            _plainColor = plainColor;
            _random = random;
        }

        public ISearchAndReplaceService Create(MosaicTypeEnum type)
        {
            switch (type)
            {
                case MosaicTypeEnum.AdjustHue:
                    return _adjusthue();

                case MosaicTypeEnum.Classic:
                    return _classic();

                case MosaicTypeEnum.PlainColor:
                    return _plainColor();

                case MosaicTypeEnum.Random:
                    return _random();

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}