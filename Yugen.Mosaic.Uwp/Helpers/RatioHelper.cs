using System;

namespace Yugen.Mosaic.Uwp.Helpers
{
    public static class RatioHelper
    {
        public static Tuple<int, int> Convert(int width, int height, int newHeight, int newWidth)
        {
            //calculate the ratio
            var ratio = (double)width / (double)height;

            //set height of image to boxHeight and check if resulting width is less than boxWidth, 
            //else set width of image to boxWidth and calculate new height
            return (int)(newHeight * ratio) <= newWidth
                ? new Tuple<int, int>((int)(newHeight * ratio), newHeight)
                : new Tuple<int, int>(newWidth, (int)(newWidth / ratio));
        }
    }
}
