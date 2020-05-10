using Yugen.Mosaic.Uwp.Extensions;

namespace Yugen.Mosaic.Uwp.Models
{
    public class MosaicType
    {
        public MosaicType(MosaicTypeEnum mosaicTypeEnum)
        {
            MosaicTypeEnum = mosaicTypeEnum;
        }

        public MosaicTypeEnum MosaicTypeEnum { get; set; }

        public override string ToString() => MosaicTypeEnum.GetStringRepresentation();
    }
}
