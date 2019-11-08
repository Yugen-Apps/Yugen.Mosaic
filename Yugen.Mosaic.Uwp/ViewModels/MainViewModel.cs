using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Yugen.Toolkit.Uwp.ViewModels;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Toolkit.Uwp.Helpers;
using System.IO;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Helpers;

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


        private int tileWidth = 40;
        public int TileWidth
        {
            get { return tileWidth; }
            set 
            { 
                Set(ref tileWidth, value);
                tileSize.Width = tileWidth;
            }
        }

        private int tileHeight = 40;
        public int TileHeight
        {
            get { return tileHeight; }
            set 
            { 
                Set(ref tileHeight, value);
                tileSize.Height = tileHeight;
            }
        }

        private Size tileSize = new Size(40, 40);

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

        private int outputWidth = 200;
        public int OutputWidth
        {
            get { return outputWidth; }
            set
            {
                Set(ref outputWidth, value);
                outputSize.Width = outputWidth;
            }
        }

        private int outputHeight = 200;
        public int OutputHeight
        {
            get { return outputHeight; }
            set
            {
                Set(ref outputHeight, value);
                outputSize.Height = outputHeight;
            }
        }

        private Size outputSize = new Size(200, 200);

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
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
            MosaicService mosaicClass = new MosaicService();

            var resizedMaster = masterImageSource.Resize(200, 200, WriteableBitmapExtensions.Interpolation.Bilinear);
            LockBitmap test = mosaicClass.GenerateMosaic(resizedMaster, outputSize, tileList.ToList(), tileSize);
            //LockBitmap test = await mosaicClass.GenerateMosaic(masterImageSource, outputSize, tileList, tileSize, true);

            OutputImageSource = test.output;
            IsLoading = false;
        }

        public async void SaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var file = await FilePickerHelper.SaveFile("filename", "Image", ".jpg");
            await WriteableBitmapHelper.WriteableBitmapToStorageFile(file, outputImageSource, FileFormat.Jpeg);
        }

    }
}
