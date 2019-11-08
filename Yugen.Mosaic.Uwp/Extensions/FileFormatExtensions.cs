using Yugen.Mosaic.Uwp.Enums;

namespace Yugen.Mosaic.Uwp.Extensions
{
    public static class FileFormatExtensions
    {
        public static string FileFormatToString(this FileFormat fileFormat) => $".{fileFormat.ToString().ToLower()}";
    }
}
