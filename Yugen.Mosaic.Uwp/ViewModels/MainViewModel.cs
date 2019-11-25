using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private BitmapImage _masterBmpSource = new BitmapImage();
        public BitmapImage MasterBpmSource
        {
            get { return _masterBmpSource; }
            set { Set(ref _masterBmpSource, value); }
        }

        private bool _isAddMasterUIVisible = true;
        public bool IsAddMasterUIVisible
        {
            get { return _isAddMasterUIVisible; }
            set { Set(ref _isAddMasterUIVisible, value); }
        }

        private int _tileWidth = 50;
        public int TileWidth
        {
            get { return _tileWidth; }
            set { Set(ref _tileWidth, value); }
        }

        private int _tileHeight = 50;
        public int TileHeight
        {
            get { return _tileHeight; }
            set { Set(ref _tileHeight, value); }
        }

        private Size _tileSize => new Size(_tileWidth, _tileHeight);

        private ObservableCollection<TileBmp> _tileBmpCollection = new ObservableCollection<TileBmp>();
        public ObservableCollection<TileBmp> TileBmpCollection
        {
            get { return _tileBmpCollection; }
            set { Set(ref _tileBmpCollection, value); }
        }


        private BitmapImage _outputBmpSource = new BitmapImage();
        public BitmapImage OutputBmpSource
        {
            get { return _outputBmpSource; }
            set { Set(ref _outputBmpSource, value); }
        }

        private int _outputWidth = 1000;
        public int OutputWidth
        {
            get { return _outputWidth; }
            set { Set(ref _outputWidth, value); }
        }

        private int _outputHeight = 1000;
        public int OutputHeight
        {
            get { return _outputHeight; }
            set { Set(ref _outputHeight, value); }
        }

        private Size outputSize => new Size(_outputWidth, _outputHeight);


        public List<MosaicType> MosaicTypeList { get; set; } = new List<MosaicType>
        {
            new MosaicType { Id=0, Title="Classic" },
            new MosaicType { Id=1, Title="AdjustHue" },
            new MosaicType { Id=2, Title="Plain Color" }
        };

        private MosaicType _selectedMosaicType;
        public MosaicType SelectedMosaicType
        {
            get { return _selectedMosaicType; }
            set { Set(ref _selectedMosaicType, value); }
        }


        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { Set(ref _isLoading, value); }
        }

        private MosaicService _mosaicService = new MosaicService();

        private Image<Rgba32> _outputImage;


        public MainViewModel()
        {
            SelectedMosaicType = MosaicTypeList[0];
        }


        public void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            IsAddMasterUIVisible = true;
        }

        public void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            UpdateIsAddMasterUIVisible();
        }

        private void UpdateIsAddMasterUIVisible()
        {
            IsAddMasterUIVisible = (MasterBpmSource.PixelWidth > 0 && MasterBpmSource.PixelHeight > 0) ? false : true;
        }


        public async void AddMasterGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var masterFile = await FilePickerHelper.OpenFile(new List<string> { ".jpg", ".png" });
            if (masterFile != null)
            {
                using (var inputStream = await masterFile.OpenReadAsync())
                using (var stream = inputStream.AsStreamForRead())
                {
                    var image = _mosaicService.AddMasterImage(stream);

                    using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 400))
                    {
                        InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);

                        await MasterBpmSource.SetSourceAsync(outputStream);
                    }
                }
            }

            UpdateIsAddMasterUIVisible();
        }

        public async void AddTilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var files = await FilePickerHelper.OpenFiles(new List<string> { ".jpg", ".png" });
            if (files == null)
                return;

            IsLoading = true;

            foreach (var file in files)
            {
                using (var inputStream = await file.OpenReadAsync())
                using (var stream = inputStream.AsStreamForRead())
                {
                    var image = _mosaicService.AddTileImage(stream);

                    using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 200))
                    {
                        InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);

                        var bmp = new BitmapImage();
                        await bmp.SetSourceAsync(outputStream);

                        TileBmpCollection.Add(new TileBmp(file.Name, bmp));
                    }
                }
            }

            IsLoading = false;
        }


        public async void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IsLoading = true;

            await Task.Delay(1);

            _outputImage = _mosaicService.GenerateMosaic(outputSize, _tileSize, SelectedMosaicType.Id);

            if (_outputImage != null)
            {
                InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(_outputImage);
                await OutputBmpSource.SetSourceAsync(outputStream);
            }

            IsLoading = false;
        }

        public async void SaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fileFormat = FileFormat.Jpg;
            var file = await FilePickerHelper.SaveFile("Mosaic", "Image", fileFormat.FileFormatToString());
            if (file == null || _outputImage == null)
                return;

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                _outputImage.SaveAsJpeg(stream.AsStreamForWrite());
            }
        }

        public void ResetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _mosaicService.Reset();

            MasterBpmSource = null;
            TileBmpCollection.Clear();
        }
    }
}
