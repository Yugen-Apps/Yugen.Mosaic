using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private WriteableBitmap masterImageSource;
        public WriteableBitmap MasterImageSource
        {
            get { return masterImageSource; }
            set { Set(ref masterImageSource, value); }
        }


        private int tileWidth = 50;
        public int TileWidth
        {
            get { return tileWidth; }
            set
            {
                Set(ref tileWidth, value);
                tileSize.Width = tileWidth;
            }
        }

        private int tileHeight = 50;
        public int TileHeight
        {
            get { return tileHeight; }
            set
            {
                Set(ref tileHeight, value);
                tileSize.Height = tileHeight;
            }
        }

        private Size tileSize = new Size(50, 50);

        private ObservableCollection<WriteableBitmap> tileList = new ObservableCollection<WriteableBitmap>();
        public ObservableCollection<WriteableBitmap> TileList
        {
            get { return tileList; }
            set { Set(ref tileList, value); }
        }


        private WriteableBitmap outputImageSource;
        public WriteableBitmap OutputImageSource
        {
            get { return outputImageSource; }
            set { Set(ref outputImageSource, value); }
        }

        private int outputWidth = 100;
        public int OutputWidth
        {
            get { return outputWidth; }
            set
            {
                Set(ref outputWidth, value);
                outputSize.Width = outputWidth;
            }
        }

        private int outputHeight = 100;
        public int OutputHeight
        {
            get { return outputHeight; }
            set
            {
                Set(ref outputHeight, value);
                outputSize.Height = outputHeight;
            }
        }

        private Size outputSize = new Size(100, 100);

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }

        private bool isAdjustHue;
        public bool IsAdjustHue
        {
            get { return isAdjustHue; }
            set { Set(ref isAdjustHue, value); }
        }

        public async void AddMasterButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var masterFile = await FilePickerHelper.OpenFile(new List<string> { ".jpg", ".png" });
            if (masterFile == null)
                return;
            using (var inputStream = await masterFile.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                MasterImageSource = await BitmapFactory.FromStream(stream);
            }
        }

        public async void AddTilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var files = await FilePickerHelper.OpenFiles(new List<string> { ".jpg", ".png" });
            if (files == null)
                return;

            foreach (var file in files)
            {
                using (var inputStream = await file.OpenReadAsync())
                using (var stream = inputStream.AsStreamForRead())
                {
                    var bmp = await BitmapFactory.FromStream(stream);
                    tileList.Add(bmp);
                }
            }
        }

        public void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IsLoading = true;
            var resizedMaster = masterImageSource.Resize(outputWidth, outputHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

            MosaicService mosaicClass = new MosaicService();
            LockBitmap mosaicBmp = mosaicClass.GenerateMosaic(resizedMaster, outputSize, tileList.ToList(), tileSize, isAdjustHue);

            OutputImageSource = mosaicBmp.Output;
            IsLoading = false;
        }

        public async void SaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fileFormat = FileFormat.Jpg;
            var file = await FilePickerHelper.SaveFile("Mosaic", "Image", fileFormat.FileFormatToString());
            if (file == null)
                return;

            await WriteableBitmapHelper.WriteableBitmapToStorageFile(file, outputImageSource, fileFormat);
        }

    }
}
