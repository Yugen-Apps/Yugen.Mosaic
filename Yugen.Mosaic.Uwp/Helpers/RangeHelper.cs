namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class RangeHelper
    {
        /// <summary>
        /// convert from one range to another
        /// oldMin : oldValue : oldMax = newMin : newValue : newMax
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="oldMin"></param>
        /// <param name="oldMax"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns>newValue</returns>
        public static double Convert(double oldValue, double oldMin, double oldMax, double newMin, double newMax) =>
            (oldValue - oldMin) * (newMax - newMin) /
                (oldMax - oldMin) + newMin;
    }
}