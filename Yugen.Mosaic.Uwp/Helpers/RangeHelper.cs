namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class RangeHelper
    {
        /// <summary>
        /// min1 : min2 = max1 : max2
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="min1"></param>
        /// <param name="max1"></param>
        /// <param name="min2"></param>
        /// <param name="max2"></param>
        /// <returns>x1 -> x2</returns>
        public static double Range(double x1, double min1, double max1, double min2, double max2) =>
            (x1 - min1) * (max2 - min2) /
                (max1 - min1) + min2;
    }
}