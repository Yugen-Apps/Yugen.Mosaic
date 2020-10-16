using Yugen.Mosaic.Uwp.Enums;

namespace Yugen.Mosaic.Uwp.Interfaces
{
    public interface ISearchAndReplaceServiceFactory
    {
        ISearchAndReplaceService Create(MosaicTypeEnum type);
    }
}