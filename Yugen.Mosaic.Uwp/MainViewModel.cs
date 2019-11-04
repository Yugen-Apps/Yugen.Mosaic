using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Yugen.Toolkit.Standard.Extensions;
using Yugen.Toolkit.Uwp.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private string masterImageSource = "ms-appx:///";
        public string MasterImageSource
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

        private Windows.UI.Xaml.Media.Imaging.WriteableBitmap outputImageSource;
        public Windows.UI.Xaml.Media.Imaging.WriteableBitmap OutputImageSource
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

        public void AddMasterButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MasterImageSource = "ms-appx:///Assets/Images/master.png";
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

            //outputImage.Source = outputImageSource;

            //image.Source = mosaicClass.OutputBmp;
            //image.Source = mosaicClass.InputBmpList[0];
        }
    }
}
