using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace Yugen.Mosaic.Uwp.Models
{
    public class LockBitmap
    {
        public WriteableBitmap Output { get; set; }

        private readonly int width;
        private readonly int height;
        private readonly int depth = 32;
        private readonly byte[] pixels;

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public LockBitmap(Size outputSize)
        {
            try
            {
                // Set width and height of bitmap
                width = (int)outputSize.Width;
                height = (int)outputSize.Height;

                // get total locked pixels count
                int PixelCount = width * height;

                // create byte array to copy pixel values
                int step = depth / 8;
                pixels = new byte[PixelCount * step];

                Output = BitmapFactory.New(width, height);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void Resize()
        //{
            //source = masterBmp.Clone();
            //source = masterBmp.Resize(Width, Height, WriteableBitmapExtensions.Interpolation.Bilinear);
            // Get width and height of bitmap
            //Width = source.PixelWidth;
            //Height = source.PixelHeight;
        //}

        //private void CalculateDeoth()
        //{
            // get source bitmap pixel format size
            //Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

            // Check if bpp (Bits Per Pixel) is 8, 24, or 32
            //if (Depth != 8 && Depth != 24 && Depth != 32)
            //{
            //    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
            //}
        //}

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            Color clr = new Color();

            // Get color components count
            int cCount = depth / 8;

            // Get start index of the specified pixel
            int i = (y * width + x) * cCount;

            if (i > pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                clr = Color.FromArgb(255, r, g, b);
            }
            if (depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = pixels[i];
                clr = Color.FromArgb(255, c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            int cCount = depth / 8;

            // Get start index of the specified pixel
            int i = (y * width + x) * cCount;

            if (depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = color.A;
            }
            if (depth == 24) // For 24 bpp set Red, Green and Blue
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
            }
            if (depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                pixels[i] = color.B;
            }

            Output.SetPixel(x, y, color);
        }
    }
}
