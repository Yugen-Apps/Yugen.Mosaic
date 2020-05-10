using System.ComponentModel;

namespace Yugen.Mosaic.Uwp.Models
{
    public enum MosaicTypeEnum
    {
        [Description("Classic")]
        Classic,
        [Description("Random")]
        Random,
        [Description("Adjust Hue")]
        AdjustHue,
        [Description("Plain Color")]
        PlainColor
    }
}
