using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Interfaces;

namespace Yugen.Mosaic.Uwp.Services
{
    public interface ISearchAndReplaceServiceFactory
    {
        ISearchAndReplaceService Create(MosaicTypeEnum type);
    }
}