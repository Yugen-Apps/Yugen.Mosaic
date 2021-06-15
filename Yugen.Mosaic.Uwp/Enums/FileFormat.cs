using System.ComponentModel;

namespace Yugen.Mosaic.Uwp.Enums
{
    public enum FileFormat
    {
        [Description(".jpg")]
        Jpg,

        [Description(".jpeg")]
        Jpeg,

        [Description(".png")]
        Png,

        [Description(".bmp")]
        Bmp,

        [Description(".tiff")]
        Tiff,

        [Description(".gif")]
        Gif,

        [Description(".txt")]
        Txt
    }
}