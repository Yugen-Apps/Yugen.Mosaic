using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private BitmapImage _masterBmpSource = new BitmapImage();

        public BitmapImage MasterBpmSource
        {
            get => _masterBmpSource;
            set => Set(ref _masterBmpSource, value);
        }

        private bool _isAddMasterUIVisible = true;

        public bool IsAddMasterUIVisible
        {
            get => _isAddMasterUIVisible;
            set => Set(ref _isAddMasterUIVisible, value);
        }


        private int _tileWidth = 25;

        public int TileWidth
        {
            get => _tileWidth;
            set => Set(ref _tileWidth, value);
        }

        private int _tileHeight = 25;

        public int TileHeight
        {
            get => _tileHeight;
            set => Set(ref _tileHeight, value);
        }

        private Size TileSize => new Size(_tileWidth, _tileHeight);

        private ObservableCollection<TileBmp> _tileBmpCollection = new ObservableCollection<TileBmp>();

        public ObservableCollection<TileBmp> TileBmpCollection
        {
            get => _tileBmpCollection;
            set => Set(ref _tileBmpCollection, value);
        }


        private BitmapImage _outputBmpSource = new BitmapImage();

        public BitmapImage OutputBmpSource
        {
            get => _outputBmpSource;
            set => Set(ref _outputBmpSource, value);
        }

        private int _outputWidth = 1000;

        public int OutputWidth
        {
            get => _outputWidth;
            set => Set(ref _outputWidth, value);
        }

        private int _outputHeight = 1000;

        public int OutputHeight
        {
            get => _outputHeight;
            set => Set(ref _outputHeight, value);
        }

        private Size OutputSize => new Size(_outputWidth, _outputHeight);


        private bool _isAlignmentGridVisibile = true;

        public bool IsAlignmentGridVisibile
        {
            get => _isAlignmentGridVisibile;
            set => Set(ref _isAlignmentGridVisibile, value);
        }


        public List<MosaicType> MosaicTypeList { get; set; } = new List<MosaicType>
        {
            new MosaicType {Id = 0, Title = "Classic"},
            new MosaicType {Id = 1, Title = "Random"},
            new MosaicType {Id = 2, Title = "AdjustHue"},
            new MosaicType {Id = 3, Title = "Plain Color"}
        };

        private MosaicType _selectedMosaicType;

        public MosaicType SelectedMosaicType
        {
            get => _selectedMosaicType;
            set => Set(ref _selectedMosaicType, value);
        }


        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private readonly MosaicService _mosaicService = new MosaicService();

        private Image<Rgba32> _outputImage;


        private bool _isTeachingTipOpen;

        public bool IsTeachingTipOpen
        {
            get => _isTeachingTipOpen;
            set => Set(ref _isTeachingTipOpen, value);
        }

        private string _teachingTipTitle;

        public string TeachingTipTitle
        {
            get => _teachingTipTitle;
            set => Set(ref _teachingTipTitle, value);
        }

        private string _teachingTipSubTitle;

        public string TeachingTipSubTitle
        {
            get => _teachingTipSubTitle;
            set => Set(ref _teachingTipSubTitle, value);
        }

        private FrameworkElement _teachingTipTarget;

        public FrameworkElement TeachingTipTarget
        {
            get => _teachingTipTarget;
            set => Set(ref _teachingTipTarget, value);
        }


        public MainViewModel()
        {
            SelectedMosaicType = MosaicTypeList[0];
        }


        public void Grid_PointerEntered(object sender, PointerRoutedEventArgs e) => IsAddMasterUIVisible = true;

        public void Grid_PointerExited(object sender, PointerRoutedEventArgs e) => UpdateIsAddMasterUIVisible();

        private void UpdateIsAddMasterUIVisible() => IsAddMasterUIVisible = (MasterBpmSource.PixelWidth > 0 && MasterBpmSource.PixelHeight > 0) ? false : true;


        public void AddMasterGrid_Tapped(object sender, TappedRoutedEventArgs e) => AddMaster().FireAndForgetSafeAsync();

        public void AddTilesButton_Click(object sender, RoutedEventArgs e) => AddTiles((Button)sender).FireAndForgetSafeAsync();

        public void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TileBmp item)
            {
                RemoveTile(item).FireAndForgetSafeAsync();
            }
        }

        public void GenerateButton_Click(object sender, RoutedEventArgs e) => Generate((Button)sender).FireAndForgetSafeAsync();

        public void SaveButton_Click(object sender, RoutedEventArgs e) => Save((Button)sender).FireAndForgetSafeAsync();

        public void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _mosaicService.Reset();

            MasterBpmSource = new BitmapImage();
            TileBmpCollection = new ObservableCollection<TileBmp>();

            GC.Collect();

            UpdateIsAddMasterUIVisible();
        }

        public void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            OnboardingHelper.IsDisabled = false;
            ShowTeachingTip();
        }

        public void SettingsButton_Click(object sender, RoutedEventArgs e) => Settings().FireAndForgetSafeAsync();


        private async Task AddMaster()
        {
            IsLoading = true;

            StorageFile masterFile = await FilePickerHelper.OpenFile(
                new List<string> { ".jpg", ".png" },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (masterFile != null)
            {
                using (IRandomAccessStreamWithContentType inputStream = await masterFile.OpenReadAsync())
                using (Stream stream = inputStream.AsStreamForRead())
                {
                    Image<Rgba32> image = _mosaicService.AddMasterImage(stream);

                    using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 400))
                    {
                        InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);
                        await MasterBpmSource.SetSourceAsync(outputStream);
                    }

                    Tuple<int, int> newSize = RatioHelper.Convert(image.Width, image.Height, OutputSize.Width, OutputSize.Height);
                    OutputWidth = newSize.Item1;
                    OutputHeight = newSize.Item2;
                }
            }

            UpdateIsAddMasterUIVisible();

            IsLoading = false;
        }

        private async Task AddTiles(Button button)
        {
            IReadOnlyList<StorageFile> files = await FilePickerHelper.OpenFiles(
                new List<string> { ".jpg", ".png" },
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            if (files == null)
            {
                return;
            }

            button.IsEnabled = false;
            IsLoading = true;

            await Task.Run(() =>
                Parallel.ForEach(files, async file =>
                {
                    using (IRandomAccessStreamWithContentType inputStream = await file.OpenReadAsync())
                    using (Stream stream = inputStream.AsStreamForRead())
                    {
                        Image<Rgba32> image = _mosaicService.AddTileImage(file.DisplayName, stream);

                        using (Image<Rgba32> copy = _mosaicService.GetResizedImage(image, 200))
                        {
                            InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(copy);

                            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                            {
                                var bmp = new BitmapImage();
                                await bmp.SetSourceAsync(outputStream);

                                TileBmpCollection.Add(new TileBmp(file.DisplayName, bmp));
                            });
                        }
                    }
                })
            );

            IsLoading = false;
            button.IsEnabled = true;
        }

        private async Task RemoveTile(TileBmp item)
        {
            await MessageDialogHelper.Confirm("Do you want to Remove this picture?",
                "",
                new UICommand("Yes",
                    action =>
                    {
                        TileBmpCollection.Remove(item);
                        _mosaicService.RemoveTileImage(item.Name);
                    }),
                new UICommand("No"));
        }

        private async Task Generate(Button button)
        {
            button.IsEnabled = false;
            IsLoading = true;

            await Task.Run(() =>
                _outputImage = _mosaicService.GenerateMosaic(OutputSize, TileSize, SelectedMosaicType.Id));

            if (_outputImage != null)
            {
                InMemoryRandomAccessStream outputStream = _mosaicService.GetStream(_outputImage);
                await OutputBmpSource.SetSourceAsync(outputStream);
            }

            IsLoading = false;
            button.IsEnabled = true;
        }

        private async Task Save(Button button)
        {
            button.IsEnabled = false;
            IsLoading = true;

            var fileTypes = new Dictionary<string, List<string>>()
            {
                {FileFormat.Png.ToString(), new List<string>() {FileFormat.Png.FileFormatToString()}},
                {FileFormat.Jpg.ToString(), new List<string>() {FileFormat.Jpg.FileFormatToString()}}
            };

            StorageFile file = await FilePickerHelper.SaveFile("Mosaic", fileTypes,
                Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);

            if (file == null || _outputImage == null)
            {
                return;
            }

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                switch (file.FileType)
                {
                    case ".png":
                        _outputImage.SaveAsPng(stream.AsStreamForWrite());
                        break;
                    default:
                        _outputImage.SaveAsJpeg(stream.AsStreamForWrite());
                        break;
                }
            }

            IsLoading = false;
            button.IsEnabled = true;
        }

        private async Task Settings()
        {
            var d = new SettingsDialog();
            await d.ShowAsync();
        }


        public void ShowTeachingTip()
        {
            OnboardingElement onboardingElement = OnboardingHelper.ShowTeachingTip();
            if (onboardingElement == null)
            {
                return;
            }

            TeachingTipTitle = onboardingElement.Title;
            TeachingTipSubTitle = onboardingElement.Subtitle;
            TeachingTipTarget = onboardingElement.Target;
            IsTeachingTipOpen = true;
        }

        public void TeachingTip_Closing(TeachingTip sender, TeachingTipClosingEventArgs args)
        {
            TeachingTipTitle = "";
            TeachingTipSubTitle = "";
            TeachingTipTarget = null;
            IsTeachingTipOpen = false;
        }

        public void TeachingTip_Closed(TeachingTip sender, TeachingTipClosedEventArgs args) => ShowTeachingTip();

        public void TeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            OnboardingHelper.IsDisabled = true;
            IsTeachingTipOpen = false;
        }
    }
}