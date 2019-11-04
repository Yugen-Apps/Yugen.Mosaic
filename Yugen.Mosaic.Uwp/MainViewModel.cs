using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Yugen.Toolkit.Standard.Extensions;
using Yugen.Toolkit.Uwp.ViewModels;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Toolkit.Uwp.Helpers;
using Windows.Storage;
using System.IO;

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

        private Size tileSize = new Size(50, 50);
        public Size TileSize
        {
            get { return tileSize; }
            set { Set(ref tileSize, value); }
        }

        private ObservableCollection<string> tileList = new ObservableCollection<string>();
        public ObservableCollection<string> TileList
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

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }

        public async void AddMasterButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var masterFile = await FilePickerHelper.OpenFile(new List<string> { ".jpg", ".png" });
            using (var inputStream = await masterFile.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                MasterImageSource = await BitmapFactory.FromStream(stream);
            }
        }

        public void AddTilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tileList.AddRange(new List<string>
            {
                "ms-appx:///Assets/Images/1.png",
                "ms-appx:///Assets/Images/2.png",
                "ms-appx:///Assets/Images/3.png",
                "ms-appx:///Assets/Images/4.png"
            });
        }

        public async void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IsLoading = true;
            MosaicClass mosaicClass = new MosaicClass();

            LockBitmap test = await mosaicClass.GenerateMosaic(masterImageSource, tileList.ToList(), tileSize);
            //LockBitmap test = await mosaicClass.GenerateMosaic(masterBmp, tileList, tileSize, true);

            OutputImageSource = test.output;
            IsLoading = false;

            //await FilePickerHelper.SaveFile("filename", "Image", ".jpg");
        }
    }
}
