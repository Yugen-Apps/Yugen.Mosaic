using System.ComponentModel;

namespace Yugen.Mosaic.Uwp.Enums
{
    public enum FileFormat
    {
        [Description(".jpg")]
        Jpg,

        [Description(".png")]
        Png,

        [Description(".bmp")]
        Bmp,

        [Description(".tiff")]
        Tiff,

        [Description(".gif")]
        Gif
    }
}